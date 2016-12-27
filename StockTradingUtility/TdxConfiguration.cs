using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace StockTrading.Utility
{
    public class TdxConfiguration : ConfigurationSection
    {
        private const string ServerPropertyName = "server";
        private const string PortPropertyName = "port";
        private const string ProtocolVersionPropertyName = "protocolVersion";
        private const string YybIdPropertyName = "yybid";
        private const string AccountPropertyName = "account";
        private const string AccountTypePropertyName = "accountType";
        private const string PasswordPropertyName = "password";

        [ConfigurationProperty(ServerPropertyName, IsRequired = true)]
        public string Server
        {
            get { return (string)this[ServerPropertyName]; }
            set { this[ServerPropertyName] = value; }
        }

        [ConfigurationProperty(PortPropertyName, IsRequired = true)]
        [IntegerValidator(MinValue= 0, MaxValue = 65535)]
        public short Port
        {
            get { return (short)this[PortPropertyName]; }
            set { this[PortPropertyName] = value; }
        }

        [ConfigurationProperty(ProtocolVersionPropertyName, IsRequired = true)]
        public string ProtocalVersion
        {
            get { return (string)this[ProtocolVersionPropertyName]; }
            set { this[ProtocolVersionPropertyName] = value; }
        }

        [ConfigurationProperty(YybIdPropertyName, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 256)]
        public short YybId
        {
            get { return (short)this[YybIdPropertyName]; }
            set { this[YybIdPropertyName] = value; }
        }

        [ConfigurationProperty(AccountPropertyName, IsRequired = true)]
        public string Account
        {
            get { return (string)this[AccountPropertyName]; }
            set { this[AccountPropertyName] = value; }
        }

        [ConfigurationProperty(AccountPropertyName, IsRequired = true)]
        [IntegerValidator(MinValue = 0, MaxValue = 256)]
        public short AccountType
        {
            get { return (short)this[AccountPropertyName]; }
            set { this[AccountPropertyName] = value; }
        }

        [ConfigurationProperty(PasswordPropertyName, IsRequired = true)]
        public string Password
        {
            get { return (string)this[PasswordPropertyName]; }
            set { this[PasswordPropertyName] = value; }
        }
    }
}
