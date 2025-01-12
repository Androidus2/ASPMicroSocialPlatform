using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using ASPMicroSocialPlatform.Models;
using ASPMicroSocialPlatform.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;




namespace ASPMicroSocialPlatform.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        async Task BroadCastToAGroup(int groupId, string methodName, object data)
        {
            var members = await _context.Groups
                .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            foreach (var member in members.UserGroups)
                await Clients.User(member.UserId).SendAsync(methodName, data);

            // also send it to the admins who are not in the group
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                if (members.UserGroups.FirstOrDefault(ug => ug.UserId == admin.Id) == null)
                    await Clients.User(admin.Id).SendAsync(methodName, data);
            }
        }


        public async Task SendMessageToUsers(Message chatMessage)
        {
            Console.WriteLine("Received message from user: " + chatMessage.UserId);
            Console.WriteLine("Message: " + chatMessage.Content);
            Console.WriteLine("Timestamp: " + chatMessage.Timestamp);

            chatMessage.Timestamp = DateTime.Now;

            // check if the sender is in the group still
            var userId = _userManager.GetUserId(Context.User);
            if (userId != chatMessage.UserId)
            {
                Console.WriteLine("Userul curent nu este cel care trimite mesajul");
                return;
            }

            // find user name
            var user = await _context.ApplicationUsers.FindAsync(chatMessage.UserId);
            var userName = user.FirstName + " " + user.LastName;
            var userProfilePicture = user.ProfilePicture;

            // save message to database
            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // search the database for the message we just saved (to get the id)
            var messageIdDB = await _context.Messages.FirstOrDefaultAsync(m => m.Content == chatMessage.Content && m.UserId == chatMessage.UserId && m.Timestamp == chatMessage.Timestamp);
            var messageId = messageIdDB.Id;

            ChatMessage chatMessageToSend = new ChatMessage
            {
                Id = messageId,
                GroupId = chatMessage.GroupId,
                UserId = userId,
                UserName = userName,
                UserProfilePicture = userProfilePicture,
                Message = chatMessage.Content,
                Timestamp = chatMessage.Timestamp
            };

            await BroadCastToAGroup((int)chatMessage.GroupId, "ReceiveMessage", chatMessageToSend);

        }


        public async Task EditMessage(Message chatMessage)
        {
            Console.WriteLine("Received message from user: " + chatMessage.UserId);
            Console.WriteLine("Message: " + chatMessage.Content);
            var message = await _context.Messages.FindAsync(chatMessage.Id);
            if (message == null)
            {
                return;
            }

            var userId = _userManager.GetUserId(Context.User);

            // if the user is not the owner of the message or the admin, return
            // if the user is no longer in the group, return

            var U = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(Context.User), "Admin");

            if (message.UserId != userId && !U)
            {
                return;
            }

            var group = await _context.Groups.FindAsync(message.GroupId);
            var userGroup = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == group.Id && ug.UserId == userId);
            if (userGroup == null && !U)
            {
                return;
            }

            message.Content = chatMessage.Content;
            await _context.SaveChangesAsync();

            var userName = (await _context.ApplicationUsers.FindAsync(userId)).FirstName + " " + (await _context.ApplicationUsers.FindAsync(userId)).LastName;

            ChatMessage chatMessageToSend = new ChatMessage
            {
                Id = chatMessage.Id,
                GroupId = message.GroupId,
                UserId = userId,
                UserName = userName,
                Message = chatMessage.Content,
                Timestamp = chatMessage.Timestamp
            };

            await BroadCastToAGroup((int)chatMessage.GroupId, "ReceiveEdit", chatMessageToSend);
        }


        public async Task DeleteMessage(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return;
            }

            var userId = _userManager.GetUserId(Context.User);

            // if the user is not the owner of the message or the admin, return
            // if the user is no longer in the group, return

            var U = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(Context.User), "Admin");

            if (message.UserId != userId && !U)
            {
                return;
            }

            var group = await _context.Groups.FindAsync(message.GroupId);
            var userGroup = await _context.UserGroups.FirstOrDefaultAsync(ug => ug.GroupId == group.Id && ug.UserId == userId);
            if (userGroup == null && !U)
            {
                return;
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            await BroadCastToAGroup((int)message.GroupId, "ReceiveDelete", messageId);
        }
    }
}