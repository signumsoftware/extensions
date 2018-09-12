using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Entities.Authorization;
using Signum.Entities.Processes;
using Signum.Utilities;
using Signum.Entities;
using Signum.Entities.Mailing;
using Signum.Entities.Basics;
using System.Globalization;
using System.ComponentModel;
using Signum.Entities.DynamicQuery;
using System.Net.Mail;
using System.Linq.Expressions;
using Signum.Entities.Files;
using System.Security.Cryptography;
using Signum.Entities.Scheduler;
using Signum.Utilities.ExpressionTrees;
using System.Reflection;

namespace Signum.Entities.Mailing
{
    [Serializable, EntityKind(EntityKind.Main, EntityData.Transactional)]
    public class EmailMessageEntity : Entity, IProcessLineDataEntity
    {
        public EmailMessageEntity()
        {
            this.UniqueIdentifier = Guid.NewGuid();
        }

        [NotNullValidator]
        [CountIsValidator(ComparisonType.GreaterThan, 0)]
        public MList<EmailRecipientEntity> Recipients { get; set; } = new MList<EmailRecipientEntity>();

        [ImplementedByAll]
        public Lite<Entity> Target { get; set; }

        [NotNullValidator]
        public EmailAddressEmbedded From { get; set; }

        public Lite<EmailTemplateEntity> Template { get; set; }

        public DateTime CreationDate { get; private set; } = TimeZoneManager.Now;

        public DateTime? Sent { get; set; }

        public DateTime? ReceptionNotified { get; set; }

        [SqlDbType(Size = int.MaxValue)]
        string subject;
        [StringLengthValidator(AllowNulls = true, AllowLeadingSpaces = true, AllowTrailingSpaces = true)]
        public string Subject
        {
            get { return subject; }
            set { if (Set(ref subject, value)) CalculateHash(); }
        }

        [SqlDbType(Size = int.MaxValue)]
        string body;
        [StringLengthValidator(AllowNulls = true, MultiLine = true)]
        public string Body
        {
            get { return body; }
            set { if (Set(ref body, value)) CalculateHash(); }
        }

        static readonly char[] spaceChars = new[] { '\r', '\n', ' ' };

        void CalculateHash()
        {
            var str = subject + body;

            BodyHash = Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(str.Trim(spaceChars))));
        }

        [StringLengthValidator(AllowNulls = true, Min = 1, Max = 150)]
        public string BodyHash { get; set; }

        public bool IsBodyHtml { get; set; } = false;

        [AvoidForeignKey]
        public Lite<ExceptionEntity> Exception { get; set; }

        public EmailMessageState State { get; set; }

        public Guid? UniqueIdentifier { get; set; }

        public bool EditableMessage { get; set; } = true;

        public Lite<EmailPackageEntity> Package { get; set; }

        public Guid? ProcessIdentifier { get; set; }

        public int SendRetries { get; set; }

        [NotNullValidator, NoRepeatValidator]
        public MList<EmailAttachmentEmbedded> Attachments { get; set; } = new MList<EmailAttachmentEmbedded>();

        static StateValidator<EmailMessageEntity, EmailMessageState> validator = new StateValidator<EmailMessageEntity, EmailMessageState>(
            m => m.State, m => m.Exception, m => m.Sent, m => m.ReceptionNotified, m => m.Package)
            {
{EmailMessageState.Created,      false,         false,        false,                    null },
{EmailMessageState.Sent,         false,         true,         false,                    null },
{EmailMessageState.SentException,true,          true,         false,                    null },
{EmailMessageState.ReceptionNotified,true,      true,         true,                     null },
{EmailMessageState.Received,     false,         false,         false,                    false },
            };

        static Expression<Func<EmailMessageEntity, string>> ToStringExpression = e => e.Subject;
        [ExpressionField]
        public override string ToString()
        {
            return ToStringExpression.Evaluate(this);
        }
    }


    [Serializable]
    public class EmailReceptionMixin : MixinEntity
    {
        protected EmailReceptionMixin(Entity mainEntity, MixinEntity next) : base(mainEntity, next)
        {
        }

        public EmailReceptionInfoEmbedded ReceptionInfo { get; set; }
    }

    [Serializable]
    public class EmailReceptionInfoEmbedded : EmbeddedEntity
    {
        [UniqueIndex(AllowMultipleNulls = true)]
        [StringLengthValidator(AllowNulls = false, Min = 1, Max = 100)]
        public string UniqueId { get; set; }

        [NotNullValidator]
        public Lite<Pop3ReceptionEntity> Reception { get; set; }

        [SqlDbType(Size = int.MaxValue), NotNullable]
        public string RawContent { get; set; }

        public DateTime SentDate { get; set; }

        public DateTime ReceivedDate { get; set; }

        public DateTime? DeletionDate { get; set; }
    }

    [Serializable]
    public class EmailAttachmentEmbedded : EmbeddedEntity
    {
        public EmailAttachmentType Type { get; set; }

        FilePathEmbedded file;
        [NotNullValidator]
        public FilePathEmbedded File
        {
            get { return file; }
            set
            {
                if (Set(ref file, value))
                {
                    if (ContentId == null && File != null)
                        ContentId = Guid.NewGuid() + File.FileName;
                }
            }
        }

        [StringLengthValidator(AllowNulls = false, Min = 1, Max = 300)]
        public string ContentId { get; set; }

        public EmailAttachmentEmbedded Clone()
        {
            return new EmailAttachmentEmbedded
            {
                ContentId = ContentId,
                File = file.Clone(),
                Type = Type,
            };
        }

        internal bool Similar(EmailAttachmentEmbedded a)
        {
            return ContentId == a.ContentId || File.FileName == a.File.FileName;
        }

        public override string ToString()
        {
            return file?.ToString();
        }
    }

    public enum EmailAttachmentType
    {
        Attachment,
        LinkedResource
    }

    [Serializable]
    public class EmailRecipientEntity : EmailAddressEmbedded, IEquatable<EmailRecipientEntity>
    {
        public EmailRecipientEntity() { }

        public EmailRecipientEntity(EmailOwnerData data)
            : base(data)
        {
            Kind = EmailRecipientKind.To;
        }

        public EmailRecipientEntity(MailAddress ma, EmailRecipientKind kind) : base(ma)
        {
            this.Kind = kind;
        }

        public EmailRecipientKind Kind { get; set; }

        public new EmailRecipientEntity Clone()
        {
            return new EmailRecipientEntity
            {
                DisplayName = DisplayName,
                EmailAddress = EmailAddress,
                EmailOwner = EmailOwner,
                Kind = Kind,
            };
        }

        public bool Equals(EmailRecipientEntity other)
        {
            return base.Equals((EmailAddressEmbedded)other) && Kind == other.Kind;
        }

        public override bool Equals(object obj)
        {
            return obj is EmailAddressEmbedded && Equals((EmailAddressEmbedded)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Kind.GetHashCode();
        }

        public string BaseToString()
        {
            return base.ToString();
        }

        public override string ToString()
        {
            return "{0}: {1}".FormatWith(Kind.NiceToString(), base.ToString());
        }
    }

    public enum EmailRecipientKind
    {
        To,
        Cc,
        Bcc
    }

    [Serializable]
    public class EmailAddressEmbedded : EmbeddedEntity, IEquatable<EmailAddressEmbedded>
    {
        public EmailAddressEmbedded() { }

        public EmailAddressEmbedded(EmailOwnerData data)
        {
            EmailOwner = data.Owner;
            EmailAddress = data.Email;
            DisplayName = data.DisplayName;
        }

        public EmailAddressEmbedded(MailAddress mailAddress)
        {
            DisplayName = mailAddress.DisplayName;
            EmailAddress = mailAddress.Address;
        }

        public Lite<IEmailOwnerEntity> EmailOwner { get; set; }

        [StringLengthValidator(AllowNulls = false, Min = 3, Max = 100)]
        public string EmailAddress { get; set; }

        public bool InvalidEmail { get; set; }

        protected override string PropertyValidation(PropertyInfo pi)
        {
            if (pi.Name == nameof(EmailAddress) && !InvalidEmail && !EMailValidatorAttribute.EmailRegex.IsMatch(EmailAddress))
                return ValidationMessage._0DoesNotHaveAValid1Format.NiceToString().FormatWith("{0}", pi.NiceName());


            return base.PropertyValidation(pi);
        }

        public string DisplayName { get; set; }

        public override string ToString()
        {
            return "{0} <{1}>".FormatWith(DisplayName, EmailAddress);
        }

        public EmailAddressEmbedded Clone()
        {
            return new EmailAddressEmbedded
            {
                DisplayName = DisplayName,
                EmailAddress = EmailAddress,
                EmailOwner = EmailOwner
            };
        }

        public bool Equals(EmailAddressEmbedded other)
        {
            return other.EmailAddress == EmailAddress && other.DisplayName == DisplayName;
        }

        public override bool Equals(object obj)
        {
            return obj is EmailAddressEmbedded && Equals((EmailAddressEmbedded)obj);
        }

        public override int GetHashCode()
        {
            return (EmailAddress ?? "").GetHashCode() ^ (DisplayName ?? "").GetHashCode();
        }
    }

    public enum EmailMessageState
    {
        [Ignore]
        Created,
        Draft,
        ReadyToSend,
        RecruitedForSending,
        Sent,
        SentException,
        ReceptionNotified,
        Received,
        Outdated
    }

    public interface IEmailOwnerEntity : IEntity
    {
    }

    [DescriptionOptions(DescriptionOptions.Description | DescriptionOptions.Members)]
    public class EmailOwnerData : IEquatable<EmailOwnerData>
    {
        public Lite<IEmailOwnerEntity> Owner { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public CultureInfoEntity CultureInfo { get; set; }

        public bool Equals(EmailOwnerData other)
        {
            return Owner != null && other != null && other.Owner != null && Owner.Equals(other.Owner);
        }

        public override bool Equals(object obj)
        {
            return obj is EmailOwnerData && Equals((EmailOwnerData)obj);
        }

        public override int GetHashCode()
        {
            return Owner == null ? base.GetHashCode() : Owner.GetHashCode();
        }

        public override string ToString()
        {
            return "{0} <{1}> ({2})".FormatWith(DisplayName, Email, Owner);
        }
    }

    [AutoInit]
    public static class EmailMessageProcess
    {
        public static readonly ProcessAlgorithmSymbol CreateEmailsSendAsync;
        public static ProcessAlgorithmSymbol SendEmails;
    }

    [AutoInit]
    public static class EmailMessageOperation
    {
        public static ExecuteSymbol<EmailMessageEntity> Save;
        public static ExecuteSymbol<EmailMessageEntity> ReadyToSend;
        public static ExecuteSymbol<EmailMessageEntity> Send;
        public static ConstructSymbol<EmailMessageEntity>.From<EmailMessageEntity> ReSend;
        public static ConstructSymbol<ProcessEntity>.FromMany<EmailMessageEntity> ReSendEmails;
        public static ConstructSymbol<EmailMessageEntity>.Simple CreateMail;
        public static ConstructSymbol<EmailMessageEntity>.From<EmailTemplateEntity> CreateEmailFromTemplate;
        public static DeleteSymbol<EmailMessageEntity> Delete;
    }

    public enum EmailMessageMessage
    {
        [Description("The email message cannot be sent from state {0}")]
        TheEmailMessageCannotBeSentFromState0,
        [Description("Message")]
        Message,
        Messages,
        RemainingMessages,
        ExceptionMessages,
        DefaultFromIsMandatory,
        From,
        To,
        Attachments,
        [Description("{0} {1} requires extra parameters")]
        _01requiresExtraParameters
    }

    [Serializable, EntityKind(EntityKind.System, EntityData.Transactional), TicksColumn(false)]
    public class EmailPackageEntity : Entity, IProcessDataEntity
    {
        [StringLengthValidator(AllowNulls = true, Max = 200)]
        public string Name { get; set; }

        public override string ToString()
        {
            return "EmailPackage {0}".FormatWith(Name);
        }
    }

    [AutoInit]
    public static class EmailFileType
    {
        public static FileTypeSymbol Attachment;
    }

    [AutoInit]
    public static class AsyncEmailSenderPermission
    {
        public static PermissionSymbol ViewAsyncEmailSenderPanel;
    }
}

