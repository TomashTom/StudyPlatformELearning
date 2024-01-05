using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using StudyPlatformELearningHub.Models;


namespace StudyPlatformELearningHub.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
       

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<UserLike> UserLikes { get; set; }
        public DbSet<UserHeart> UserHearts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<SeeLaterVideo> SeeLaterVideos { get; set; } 
        public DbSet<VideoRating> VideoRatings { get; set; }
        public DbSet<EntityRole> EntityRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<VideoFile> VideoFiles { get; set; }

        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<UserBlockStatus> UserBlockStatuses { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<EmailSend> EmailSends { get; set; }
        public DbSet<EmailMessage> EmailMessages { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<UserVideoView> UserVideoViews { get; set; }



        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Question)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoFile>()
                .HasMany(v => v.Questions)
                .WithOne(q => q.Video)
                .HasForeignKey(q => q.VideoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmailSend>().Ignore(e => e.Attachment);

            modelBuilder.Entity<VideoViewModel>().HasNoKey();

            modelBuilder.Entity<SeeLaterVideo>()
             .HasOne(sv => sv.Playlist)
             .WithMany(p => p.SeeLaterVideos)
             .HasForeignKey(sv => sv.PlaylistId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserLike>()
                .HasKey(ul => new { ul.UserId, ul.CommentId });

            modelBuilder.Entity<UserHeart>()
                .HasKey(uh => new { uh.UserId, uh.CommentId });

            modelBuilder.Entity<Comment>()
                .Property(c => c.HeartCount)
                .HasColumnName("HeartCount")
                .HasAnnotation("DatabaseGenerated", "false");

            modelBuilder.Entity<Comment>()
                .Property(c => c.LikeCount)
                .HasColumnName("LikeCount")
                .HasAnnotation("DatabaseGenerated", "false");
            modelBuilder.Entity<Comment>()
                .Property(c => c.UserLiked)
                .HasColumnName("UserLiked")
                .HasAnnotation("DatabaseGenerated", "false");

            modelBuilder.Entity<Comment>()
                .Property(c => c.UserHearted)
                .HasColumnName("UserHearted")
                .HasAnnotation("DatabaseGenerated", "false");

            modelBuilder.Entity<VideoFile>()
                .Property(v => v.CourseId)
                .IsRequired(false);

            modelBuilder.Entity<Course>()
               .HasMany(c => c.Videos) // Assuming Course has a collection of VideoFiles
               .WithOne(v => v.Course) // Assuming VideoFile has a Course property
               .HasForeignKey(v => v.CourseId) // Assuming VideoFile has a CourseId foreign key
               .OnDelete(DeleteBehavior.Cascade);
           

        }




    }


}



