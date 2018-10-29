namespace MCCOMailHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            var handle = new MailHandler();

            if (handle.InitializeApp())
            {
                handle.HandlePenddingMail();
            }
        }        
    }
}
