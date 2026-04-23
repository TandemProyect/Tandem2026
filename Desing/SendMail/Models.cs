namespace SendMail
{
    public class Models
    {
        public class MailModel
        {
            private string v;


            public string From
            {
                get;
                set;
            }
            public string To
            {
                get;
                set;
            }
            public string Subject
            {
                get;
                set;
            }
            public string Body
            {
                get;
                set;
            }
        }
    }
}