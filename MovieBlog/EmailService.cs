using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace MovieBlog
{
    public class EmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
 
            emailMessage.From.Add(new MailboxAddress("Администрация сайта", "nat.movieblog@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };
             
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 465, true);
                await client.AuthenticateAsync("nat.movieblog@gmail.com", "t3n3t3n3t");
                await client.SendAsync(emailMessage);
 
                await client.DisconnectAsync(true);
            }
        }
    }
}