using Signum.Entities.Basics;
using Signum.Utilities;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Signum.Entities.Workflow
{
    [Serializable, EntityKind(EntityKind.String, EntityData.Master)]
    public class WorkflowEventEntity : Entity, IWorkflowNodeEntity, IWithModel
    {
        [StringLengthValidator(Min = 3, Max = 100)]
        public string? Name { get; set; }
        
        public string? GetName() => Name;

        [StringLengthValidator(Min = 1, Max = 100)]
        public string BpmnElementId { get; set; }

        [AvoidDumpEntity]
        public WorkflowLaneEntity Lane { get; set; }

        public WorkflowEventType Type { get; set; }

        public WorkflowTimerEmbedded? Timer { get; set; }

        public Lite<WorkflowActivityEntity>? BoundaryOf { get; set; }

        [AvoidDump]
        public WorkflowXmlEmbedded Xml { get; set; }

        [AutoExpressionField]
        public override string ToString() => As.Expression(() => Name ?? BpmnElementId);

        public ModelEntity GetModel()
        {
            var model = new WorkflowEventModel()
            {
                MainEntityType = this.Lane.Pool.Workflow.MainEntityType,
                Name = this.Name,
                Type = this.Type,
                Task = WorkflowEventTaskModel.GetModel(this),
                Timer = this.Timer,
                BpmnElementId = this.BpmnElementId,
            };
            model.CopyMixinsFrom(this);
            return model;
        }

        public void SetModel(ModelEntity model)
        {
            var wModel = (WorkflowEventModel)model;
            this.Name = wModel.Name;
            this.Type = wModel.Type;
            this.Timer = wModel.Timer;
            this.BpmnElementId = wModel.BpmnElementId;
            this.CopyMixinsFrom(wModel);
            //WorkflowEventTaskModel.ApplyModel(this, wModel.Task);
        }
    }

    [Serializable]
    public class WorkflowTimerEmbedded : EmbeddedEntity
    {
        public TimeSpanEmbedded? Duration { get; set; }

        public Lite<WorkflowTimerConditionEntity>? Condition { get; set; }

        protected override string? PropertyValidation(PropertyInfo pi)
        {
            if (pi.Name == nameof(Duration) && Duration == null && Condition == null)
                return ValidationMessage._0IsMandatoryWhen1IsNotSet.NiceToString(pi.NiceName(), NicePropertyName(() => Condition));

            if (pi.Name == nameof(Duration) && Duration != null && Condition != null)
                return ValidationMessage._0ShouldBeNullWhen1IsSet.NiceToString(NicePropertyName(() => Condition), pi.NiceName());

            return base.PropertyValidation(pi);
        }

        public WorkflowTimerEmbedded Clone()
        {
            WorkflowTimerEmbedded result = new WorkflowTimerEmbedded
            {
                Condition = this.Condition,
                Duration = this.Duration == null ? null : this.Duration.Clone(),
            };

            return result;
        }
    }

    public enum WorkflowEventType
    {
        Start,
        ScheduledStart,
        Finish,
        BoundaryForkTimer,
        BoundaryInterruptingTimer,
        IntermediateTimer,
    }


    public static class WorkflowEventTypeExtension
    {
        public static bool IsStart(this WorkflowEventType type) =>
            type == WorkflowEventType.Start ||
            type == WorkflowEventType.ScheduledStart;

        public static bool IsScheduledStart(this WorkflowEventType type) =>
            type == WorkflowEventType.ScheduledStart;

        public static bool IsFinish(this WorkflowEventType type) =>
            type == WorkflowEventType.Finish;

        public static bool IsTimer(this WorkflowEventType type) =>
            type == WorkflowEventType.BoundaryForkTimer ||
            type == WorkflowEventType.BoundaryInterruptingTimer ||
            type == WorkflowEventType.IntermediateTimer;

        public static bool IsBoundaryTimer(this WorkflowEventType type) =>
            type == WorkflowEventType.BoundaryForkTimer ||
            type == WorkflowEventType.BoundaryInterruptingTimer;
    }

    [AutoInit]
    public static class WorkflowEventOperation
    {
        public static readonly ExecuteSymbol<WorkflowEventEntity> Save;
        public static readonly DeleteSymbol<WorkflowEventEntity> Delete;
    }

    [Serializable]
    public class WorkflowEventModel : ModelEntity
    {
        [InTypeScript(Undefined = false, Null = false)]
        public TypeEntity MainEntityType { get; set; }

        [StringLengthValidator(Min = 3, Max = 100)]
        public string? Name { get; set; }

        public WorkflowEventType Type { get; set; }

        public WorkflowEventTaskModel? Task { get; set; }

        public WorkflowTimerEmbedded? Timer { get; set; }

        public string BpmnElementId { get; set; }
    }
}
