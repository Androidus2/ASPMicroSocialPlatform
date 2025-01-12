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

            ChatMessage chatMessageToSend = new ChatMessage
            {
                GroupId = chatMessage.GroupId,
                UserId = userId,
                UserName = userName,
                Message = chatMessage.Content,
                Timestamp = chatMessage.Timestamp
            };

            // find all group members 
            var members = await _context.Groups
                .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.Id == chatMessage.GroupId);

            // Send message to all users
            foreach (var member in members.UserGroups)
            {
                Console.WriteLine("Am ajuns aici !");
                Console.WriteLine("<----------------------->");
                Console.WriteLine("Member: " + member.UserId);
                Console.WriteLine("User: " + userId);
                Console.WriteLine("<----------------------->");
      


                await Clients.User(member.UserId).SendAsync("ReceiveMessage", chatMessageToSend);
            }

            // save message to database
            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();


        }
    }
}