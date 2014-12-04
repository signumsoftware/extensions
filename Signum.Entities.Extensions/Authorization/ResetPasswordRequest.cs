﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Signum.Entities;

namespace Signum.Entities.Authorization
{
    [Serializable, EntityKind(EntityKind.System, EntityData.Transactional)]
    public class ResetPasswordRequestDN : Entity
    {
        string code;
        public string Code
        {
            get { return code; }
            set { Set(ref code, value); }
        }

        UserDN user;
        [NotNullValidator]
        public UserDN User
        {
            get { return user; }
            set { Set(ref user, value); }
        }

        DateTime requestDate;
        public DateTime RequestDate
        {
            get { return requestDate; }
            set { Set(ref requestDate, value); }
        }

        bool lapsed;
        public bool Lapsed
        {
            get { return lapsed; }
            set { Set(ref lapsed, value); }
        }
    }
}
