using Supabase;                         // ← Supabase.Client lives here
using Supabase.Postgrest.Models;        // BaseModel
using Supabase.Postgrest.Attributes;    // Table / Column
using StuMoov.Models.MessageModel;
using StuMoov.Models;  
               // Response

namespace StuMoov.Services.MessageService
{
    public class MessageService
    {
        // fully-qualified so there’s no ambiguity
        private readonly Supabase.Client _supabase;

        public MessageService()
        {
            var url = "https://aqjqogrfyfagsgkmqzdj.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFxanFvZ3JmeWZhZ3Nna21xemRqIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc0NTM4Nzg0OCwiZXhwIjoyMDYwOTYzODQ4fQ.-jNRu6BK1L6cGcRKS6SzqNUOasdRUjFSxVb8spVpn3c";

            _supabase = new Supabase.Client(
                url,
                key,
                new SupabaseOptions { AutoConnectRealtime = false });

            _supabase.InitializeAsync().Wait();
        }

        public async Task<Response> SendMessageAsync(Message msg)
        {
            try
            {
                var result = await _supabase.From<Message>().Insert(msg);
                return new Response(StatusCodes.Status201Created, "Message sent", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Supabase ERROR → " + ex);   // add this line
                return new Response(StatusCodes.Status500InternalServerError, ex.Message, null);
            }
        }

public async Task<Response> GetConversationAsync(Guid u1, Guid u2)
{
    try
    {
        // messages u1 → u2
        var r1 = await _supabase
            .From<Message>()
            .Where(m => m.SenderId == u1 && m.RecipientId == u2)
            .Get();

        // messages u2 → u1
        var r2 = await _supabase
            .From<Message>()
            .Where(m => m.SenderId == u2 && m.RecipientId == u1)
            .Get();

        var combined = r1.Models
                         .Concat(r2.Models)
                         .OrderBy(m => m.SentAt)
                         .ToList();

        return new Response(StatusCodes.Status200OK,
                            "Messages fetched", combined);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Supabase ERROR → " + ex);
        return new Response(StatusCodes.Status500InternalServerError,
                            ex.Message, null);
    }
}
    }
}