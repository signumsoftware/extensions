﻿using Signum.Entities.Dynamic;
using Signum.Entities;
using Signum.Entities.Basics;
using Signum.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.ComponentModel;
using Signum.Entities.Workflow;
using System.Reflection;

namespace Signum.Entities.Workflow
{
    [Serializable, EntityKind(EntityKind.Main, EntityData.Master)]
    public class WorkflowActivityEntity : Entity, IWorkflowNodeEntity, IWithModel
    {
        [NotNullable]
        [NotNullValidator]
        public WorkflowLaneEntity Lane { get; set; }

        [SqlDbType(Size = 100), NotNullable]
        [StringLengthValidator(AllowNulls = false, Min = 3, Max = 100)]
        public string Name { get; set; }

        [NotNullable, SqlDbType(Size = 100)]
        [StringLengthValidator(AllowNulls = false, Min = 1, Max = 100)]
        public string BpmnElementId { get; set; }

        [SqlDbType(Size = 400)]
        [StringLengthValidator(AllowNulls = true, Min = 3, Max = 400, MultiLine = true)]
        public string Comments { get; set; }

        public WorkflowActivityType Type { get; set; }

        public bool RequiresOpen { get; set; }

        public WorkflowRejectEntity Reject { get; set; }

        public WorkflowTimeoutEntity Timeout { get; set; }

        [StringLengthValidator(AllowNulls = true, Min = 3, Max = 255)]
        public string ViewName { get; set; }

        [NotNullable]
        [NotNullValidator, NoRepeatValidator]
        public MList<WorkflowActivityValidationEntity> ValidationRules { get; set; } = new MList<WorkflowActivityValidationEntity>();

        [NotNullable, PreserveOrder]
        [NotNullValidator, NoRepeatValidator]
        public MList<WorkflowJumpEntity> Jumps { get; set; } = new MList<WorkflowJumpEntity>();

        [NotifyChildProperty]
        public WorkflowScriptEntity Script { get; set; }

        [NotNullable]
        [NotNullValidator]
        public WorkflowXmlEntity Xml { get; set; }
        
        [NotifyChildProperty]
        public SubWorkflowEntity SubWorkflow { get; set; }

        [SqlDbType(Size = int.MaxValue)]
        [StringLengthValidator(AllowNulls = true, MultiLine = true)]
        public string UserHelp { get; set; }

        static Expression<Func<WorkflowActivityEntity, string>> ToStringExpression = @this => @this.Name ?? @this.BpmnElementId;
        [ExpressionField]
        public override string ToString()
        {
            return ToStringExpression.Evaluate(this);
        }

        protected override string PropertyValidation(PropertyInfo pi)
        {
            if (pi.Name == nameof(SubWorkflow))
            {
                var requiresSubWorkflow = this.Type == WorkflowActivityType.CallWorkflow || this.Type == WorkflowActivityType.DecompositionWorkflow;
                if (SubWorkflow != null && !requiresSubWorkflow)
                    return ValidationMessage._0ShouldBeNull.NiceToString(pi.NiceName());

                if (SubWorkflow == null && requiresSubWorkflow)
                    return ValidationMessage._0IsNotSet.NiceToString(pi.NiceName());
            }

            if(pi.Name == nameof(Jumps))
            {
                var repated = NoRepeatValidatorAttribute.ByKey(Jumps, j => j.To);
                if (repated.HasText())
                    return ValidationMessage._0HasSomeRepeatedElements1.NiceToString(pi.NiceName(), repated);

                if (Jumps.Any(j => j.To.RefersTo(this)))
                    return WorkflowMessage.JumpsToSameActivityNotAllowed.NiceToString();
            }
            return base.PropertyValidation(pi);
        }

        public ModelEntity GetModel()
        {
            var model = new WorkflowActivityModel();
            model.WorkflowActivity = this.ToLite();
            model.Workflow = this.Lane.Pool.Workflow;
            model.MainEntityType = model.Workflow.MainEntityType;
            model.Name = this.Name;
            model.Type = this.Type;
            model.RequiresOpen = this.RequiresOpen;
            model.Reject = this.Reject;
            model.Timeout = this.Timeout;
            model.ValidationRules.AssignMList(this.ValidationRules);
            model.Jumps.AssignMList(this.Jumps);
            model.Script = this.Script;
            model.ViewName = this.ViewName;
            model.UserHelp = this.UserHelp;
            model.SubWorkflow = this.SubWorkflow;
            model.Comments = this.Comments;
            return model;
        }

        public void SetModel(ModelEntity model)
        {
            var wModel = (WorkflowActivityModel)model;
            this.Name = wModel.Name;
            this.Type = wModel.Type;
            this.RequiresOpen = wModel.RequiresOpen;
            this.Reject = wModel.Reject;
            this.Timeout = wModel.Timeout;
            this.ValidationRules.AssignMList(wModel.ValidationRules);
            this.Jumps.AssignMList(wModel.Jumps);
            this.Script = wModel.Script;
            this.ViewName = wModel.ViewName;
            this.UserHelp = wModel.UserHelp;
            this.Comments = wModel.Comments;
            this.SubWorkflow = wModel.SubWorkflow;
        }
    }

    [Serializable]
    public class WorkflowRejectEntity : EmbeddedEntity, IWorkflowTransition
    {
        public Lite<WorkflowConditionEntity> Condition { get; set; }

        public Lite<WorkflowActionEntity> Action { get; set; }
    }

    [Serializable]
    public class WorkflowTimeoutEntity : EmbeddedEntity, IWorkflowTransitionTo
    {
        [NotNullable]
        [NotNullValidator]
        public TimeSpanEntity Timeout { get; set; }

        [NotNullable, ImplementedBy(typeof(WorkflowActivityEntity), typeof(WorkflowEventEntity), typeof(WorkflowGatewayEntity))]
        [NotNullValidator]
        public Lite<IWorkflowNodeEntity> To { get; set; }

        public Lite<WorkflowActionEntity> Action { get; set; }

        Lite<WorkflowConditionEntity> IWorkflowTransition.Condition => null;
    }

    [Serializable]
    public class WorkflowJumpEntity : EmbeddedEntity, IWorkflowTransitionTo
    {
        [ImplementedBy(typeof(WorkflowActivityEntity), typeof(WorkflowEventEntity), typeof(WorkflowGatewayEntity))]
        public Lite<IWorkflowNodeEntity> To { get; set; }

        public Lite<WorkflowConditionEntity> Condition { get; set; }

        public Lite<WorkflowActionEntity> Action { get; set; }
    }

    public enum WorkflowActivityType
    {
        Task,
        Decision,
        DecompositionWorkflow,
        CallWorkflow,
        Script,
    }

    [AutoInit]
    public static class WorkflowActivityOperation
    {
        public static readonly ExecuteSymbol<WorkflowActivityEntity> Save;
        public static readonly DeleteSymbol<WorkflowActivityEntity> Delete;
    }

    [Serializable]
    public class WorkflowActivityValidationEntity : EmbeddedEntity
    {
        [NotNullable]
        [NotNullValidator]
        public Lite<DynamicValidationEntity> Rule { get; set; }

        public bool OnAccept { get; set; }
        public bool OnDecline { get; set; }

        public WorkflowActivityValidationEntity Clone()
        {
            return new WorkflowActivityValidationEntity
            {
                Rule = this.Rule,
                OnAccept = this.OnAccept,
                OnDecline = this.OnDecline
            };
        }
    }

    [Serializable]
    public class SubWorkflowEntity : EmbeddedEntity
    {
        [NotNullable]
        [NotNullValidator]
        public WorkflowEntity Workflow { get; set; }

        [NotNullable]
        [NotNullValidator, NotifyChildProperty]
        public SubEntitiesEval SubEntitiesEval { get; set; }
    }

    [Serializable]
    public class SubEntitiesEval : EvalEntity<ISubEntitiesEvaluator>
    {
        protected override CompilationResult Compile()
        {
            var decomposition = (SubWorkflowEntity)this.GetParentEntity();
            var activity = (WorkflowActivityEntity)decomposition.GetParentEntity();

            var script = this.Script.Trim();
            script = script.Contains(';') ? script : ("return " + script + ";");
            var MainEntityTypeName = activity.Lane.Pool.Workflow.MainEntityType.ToType().FullName;
            var SubEntityTypeName = decomposition.Workflow.MainEntityType.ToType().FullName;

            return Compile(DynamicCode.GetAssemblies(),
                DynamicCode.GetNamespaces() +
                    @"
                    namespace Signum.Entities.Workflow
                    {
                        class MySubEntitiesEvaluator : ISubEntitiesEvaluator
                        {
                            public List<ICaseMainEntity> GetSubEntities(ICaseMainEntity mainEntity, WorkflowTransitionContext ctx)
                            {
                                return this.Evaluate((" + MainEntityTypeName + @")mainEntity, ctx).EmptyIfNull().Cast<ICaseMainEntity>().ToList();
                            }

                            IEnumerable<" + SubEntityTypeName + "> Evaluate(" + MainEntityTypeName + @" e, WorkflowTransitionContext ctx)
                            {
                                " + script + @"
                            }
                        }                  
                    }");
        }
    }

    public interface ISubEntitiesEvaluator
    {
        List<ICaseMainEntity> GetSubEntities(ICaseMainEntity mainEntity, WorkflowTransitionContext ctx);
    }

    [Serializable]
    public class WorkflowActivityModel : ModelEntity
    {
        public Lite<WorkflowActivityEntity>  WorkflowActivity { get; set; }
        public WorkflowEntity Workflow { get; set; }

        [NotNullable]
        [NotNullValidator, InTypeScript(Undefined = false, Null = false)]
        public TypeEntity MainEntityType { get; set; }
        
        [NotNullable, SqlDbType(Size = 100)]
        [StringLengthValidator(AllowNulls = false, Min = 3, Max = 100)]
        public string Name { get; set; }

        public WorkflowActivityType Type { get; set; }

        public bool RequiresOpen { get; set; }

        public WorkflowRejectEntity Reject { get; set; }

        public WorkflowTimeoutEntity Timeout { get; set; }

        [NotNullable]
        [NotNullValidator, NoRepeatValidator]
        public MList<WorkflowActivityValidationEntity> ValidationRules { get; set; } = new MList<WorkflowActivityValidationEntity>();

        [NotNullable, PreserveOrder]
        [NotNullValidator, NoRepeatValidator]
        public MList<WorkflowJumpEntity> Jumps { get; set; } = new MList<WorkflowJumpEntity>();

        public WorkflowScriptEntity Script { get; set; }

        [StringLengthValidator(AllowNulls = true, Min = 3, Max = 255)]
        public string ViewName { get; set; }

        [SqlDbType(Size = 400)]
        [StringLengthValidator(AllowNulls = true, Min = 3, Max = 400, MultiLine = true)]
        public string Comments { get; set; }

        [SqlDbType(Size = int.MaxValue)]
        [StringLengthValidator(AllowNulls = true, MultiLine = true)]
        public string UserHelp { get; set; }

        public SubWorkflowEntity SubWorkflow { get; set; }
    }

    public enum WorkflowActivityMessage {
        [Description("Duplicate view name found: {0}")]
        DuplicateViewNameFound0,
        ChooseADestinationForWorkflowJumping,
        [Description("To use '{0}', you should save workflow")]
        ToUse0YouSouldSaveWorkflow,
    }

}
