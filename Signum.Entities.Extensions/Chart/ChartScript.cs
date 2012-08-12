﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Signum.Utilities;
using Signum.Entities.DynamicQuery;
using Signum.Entities.Files;
using System.Xml.Linq;
using System.Collections;

namespace Signum.Entities.Chart
{
    [Serializable]
    public class ChartScriptDN : Entity
    {
        [NotNullable, SqlDbType(Size = 100), UniqueIndex]
        string name;
        [StringLengthValidator(AllowNulls = false, Min = 3, Max = 100)]
        public string Name
        {
            get { return name; }
            set { SetToStr(ref name, value, () => Name); }
        }

        Lite<FileDN> icon;
        public Lite<FileDN> Icon
        {
            get { return icon; }
            set { Set(ref icon, value, () => Icon); }
        }

        [NotNullable, SqlDbType(Size = int.MaxValue)]
        string script;
        [StringLengthValidator(AllowNulls = false, Min = 3)]
        public string Script
        {
            get { return script; }
            set { Set(ref script, value, () => Script); }
        }

        GroupByChart groupBy;
        public GroupByChart GroupBy
        {
            get { return groupBy; }
            set { Set(ref groupBy, value, () => GroupBy); }
        }

        [NotifyCollectionChanged, ValidateChildProperty]
        MList<ChartScriptColumnDN> columns = new MList<ChartScriptColumnDN>();
        public MList<ChartScriptColumnDN> Columns
        {
            get { return columns; }
            set { Set(ref columns, value, () => Columns); }
        }

        static Expression<Func<ChartScriptDN, string>> ToStringExpression = e => e.name;
        public override string ToString()
        {
            return ToStringExpression.Evaluate(this);
        }

        public string ColumnsToString()
        {
            return Columns.ToString(a => a.ColumnType.ToString(), "|");
        }

        protected override string ChildPropertyValidation(ModifiableEntity sender, System.Reflection.PropertyInfo pi)
        {
            var column = sender as ChartScriptColumnDN;

            if (column != null && pi.Is(() => column.IsGroupKey))
            {
                if (column.IsGroupKey)
                {
                    if (!ChartUtils.Flag(ChartColumnType.Groupable, column.ColumnType))
                        return "{0} can not be true for {1}".Formato(pi.NiceName(), column.ColumnType.NiceToString());
                }
            }

            return base.ChildPropertyValidation(sender, pi);
        }

        protected override string PropertyValidation(System.Reflection.PropertyInfo pi)
        {
            if (pi.Is(() => GroupBy))
            {
                if (GroupBy == GroupByChart.Always || GroupBy == GroupByChart.Optional)
                {
                    if (!Columns.Any(a => a.IsGroupKey))
                        return "{0} {1} requires some key columns".Formato(pi.NiceName(), groupBy.NiceToString());
                }
            }

            return base.PropertyValidation(pi);
        }

        protected override void PreSaving(ref bool graphModified)
        {
            Columns.ForEach((c, i) => c.Index = i);

            base.PreSaving(ref graphModified);
        }

        protected override void PostRetrieving()
        {
            Columns.Sort(c => c.Index);
            
            base.PostRetrieving();
        }

        public XDocument ExportXml()
        {
            var icon = Icon == null? null: Icon.Entity;

            return new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("ChartScript",
                    new XAttribute("Name", Name),
                    new XAttribute("GroupBy", GroupBy.ToString()),
                    new XElement("Columns",
                        Columns.Select(c => new XElement("Column",
                            new XAttribute("DisplayName", c.DisplayName),
                            new XAttribute("ColumnType", c.ColumnType.ToString()),
                            c.IsOptional ? new XAttribute("IsOptional", true) : null
                         ))),
                    icon == null ? null :
                    new XElement("Icon",
                        new XAttribute("FileName", icon.FileName),
                        new XCData(Convert.ToBase64String(Icon.Entity.BinaryFile))),
                    new XElement("Script", new XCData(Script))));
                    
        }

        public void ImportXml(XDocument doc, bool force = false)
        {
            XElement script = doc.Root;

            string name = script.Attribute("Name").Value;
            GroupByChart groupBy = script.Attribute("GroupBy").Value.ToEnum<GroupByChart>();

            List<ChartScriptColumnDN> columns = script.Element("Columns").Elements("Column").Select(c => new ChartScriptColumnDN
            {
                DisplayName = c.Attribute("DisplayName").Value,
                ColumnType = c.Attribute("ColumnType").Value.ToEnum<ChartColumnType>(),
                IsOptional = c.Attribute("IsOptional").Let(a => a != null && a.Value == "True"),
            }).ToList();

            if (!IsNew && Name != name && !force)
                AsssertColumns(columns);

            this.Name = name;
            this.GroupBy = groupBy;

            this.Columns = columns.ToMList();

            this.Script = script.Elements("Script").Nodes().OfType<XCData>().Single().Value;

            var newFile = script.Element("Icon").TryCC(icon => new FileDN
            {
                FileName = icon.Attribute("FileName").Value,
                BinaryFile = Convert.FromBase64String(icon.Nodes().OfType<XCData>().Single().Value),
            });

            if (newFile == null)
            {
                Icon = null;
            }
            else
            {
                var oldFile = icon.Entity;

                if (oldFile.FileName != newFile.FileName || !AreEqual(oldFile.BinaryFile, newFile.BinaryFile))
                    Icon = oldFile.ToLiteFat();
            }
        }

        static bool AreEqual(byte[] a1, byte[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }

            return true;
        }

        private void AsssertColumns(List<ChartScriptColumnDN> columns)
        {
            string errors = Columns.ZipOrDefault(columns, (o, n) =>
            {
                if (o == null)
                {
                    if (!n.IsOptional)
                        return "Adding non optional column {0}".Formato(n.DisplayName);
                }
                else if (n == null)
                {
                    if (o.IsOptional)
                        return "Removing non optional column {0}".Formato(o.DisplayName);
                }
                else if (n.ColumnType != o.ColumnType)
                {
                    return "The column type of '{0}' ({1}) does not match with '{2}' ({3})".Formato(
                        o.DisplayName, o.ColumnType,
                        n.DisplayName, n.ColumnType);
                }

                return null;
            }).NotNull().ToString("\r\n");

            if (errors.HasText())
                throw new FormatException("The columns doesn't match: \r\n" + errors);
        }
    }

    public enum ChartScriptOperations
    {
        Clone,
        Delete
    }

    public enum GroupByChart
    {
        Always,
        Optional,
        Never
    }

    [Serializable]
    public class ChartScriptColumnDN : EmbeddedEntity       
    {
        int index;
        public int Index
        {
            get { return index; }
            set { Set(ref index, value, () => Index); }
        }

        [NotNullable, SqlDbType(Size = 80)]
        string displayName;
        [StringLengthValidator(AllowNulls = false, Min = 3, Max = 80)]
        public string DisplayName
        {
            get { return displayName; }
            set { Set(ref displayName, value, () => DisplayName); }
        }

        bool isOptional;
        public bool IsOptional
        {
            get { return isOptional; }
            set { Set(ref isOptional, value, () => IsOptional); }
        }
     
        ChartColumnType columnType;
        public ChartColumnType ColumnType
        {
            get { return columnType; }
            set { Set(ref columnType, value, () => ColumnType); }
        }

        bool isGroupKey;
        public bool IsGroupKey
        {
            get { return isGroupKey; }
            set { Set(ref isGroupKey, value, () => IsGroupKey); }
        }
    }

    public enum ChartColumnType
    {
        Decimal = 1,
        DateTime = 2,
        Integer = 4,
        Date = 8,
        String = 16, //Guid
        Entity = 32, //Enum | Boolean 

        Groupable = Integer | Date | String | Entity, 
        Magnitude = Integer | Decimal,
        Positionable = Integer | Decimal | Date | DateTime,
        GroupableAndPositionable  = Integer | Date 
    }
}
