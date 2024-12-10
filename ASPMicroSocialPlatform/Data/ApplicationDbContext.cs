using ASPMicroSocialPlatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASPMicroSocialPlatform.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<UserGroup>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.GroupId });
            // definire relatii cu modelele User si Group
            modelBuilder.Entity<UserGroup>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.UserGroups)
                .HasForeignKey(ab => ab.UserId);
            modelBuilder.Entity<UserGroup>()
                .HasOne(ab => ab.Group)
                .WithMany(ab => ab.UserGroups)
                .HasForeignKey(ab => ab.GroupId);

            // definire primary key compus
            modelBuilder.Entity<Follow>()
                .HasKey(ab => new { ab.Id, ab.FollowedId, ab.FollowerId });
            // definire relatii cu modelele User si User
            modelBuilder.Entity<Follow>()
                .HasOne(ab => ab.Follower)
                .WithMany(ab => ab.Following)
                .HasForeignKey(ab => ab.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Follow>()
                .HasOne(ab => ab.Followed)
                .WithMany(ab => ab.Followers)
                .HasForeignKey(ab => ab.FollowedId);

        }



    }
}
