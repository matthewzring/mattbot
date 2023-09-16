namespace mattbot
{
    public class Attributes
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
        public class CyberPatriotAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
        public class CCDCAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
        public class CyberDiscordAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
        public class DebugAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
        public class IgnoreAttribute : Attribute
        {
        }
    }
}
