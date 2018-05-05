﻿/// <reference path="../bpmn-js.d.ts" />
import * as React from 'react'
import { WorkflowEntitiesDictionary, WorkflowActivityModel, WorkflowActivityType, WorkflowPoolModel, WorkflowLaneModel, WorkflowConnectionModel, WorkflowEventModel, WorkflowEntity, IWorkflowNodeEntity, WorkflowMessage, WorkflowEventTaskModel, WorkflowTimerEmbedded } from '../Signum.Entities.Workflow'
import * as Modeler from "bpmn-js/lib/Modeler"
import { ModelEntity, ValidationMessage, parseLite } from '../../../../Framework/Signum.React/Scripts/Signum.Entities'
import * as Navigator from '../../../../Framework/Signum.React/Scripts/Navigator'
import * as connectionIcons from './ConnectionIcons'
import * as customRenderer from './CustomRenderer'
import * as customPopupMenu from './CustomPopupMenu'
import * as BpmnUtils from './BpmnUtils'

import "bpmn-js/dist/assets/bpmn-font/css/bpmn-embedded.css"
import "diagram-js/assets/diagram-js.css"
import "./Bpmn.css"
import { Button } from '../../../../Framework/Signum.React/Scripts/Components';
import { TypeEntity } from '../../../../Framework/Signum.React/Scripts/Signum.Entities.Basics';
import { newMListElement } from '../../../../Framework/Signum.React/Scripts/Signum.Entities';
import { TimeSpanEmbedded } from '../../Basics/Signum.Entities.Basics';
import { Dic } from '../../../../Framework/Signum.React/Scripts/Globals';

export interface BpmnModelerComponentProps {
    workflow: WorkflowEntity;
    diagramXML: string;
    entities: WorkflowEntitiesDictionary;
}

class CustomModeler extends Modeler {

}

CustomModeler.prototype._modules =
    CustomModeler.prototype._modules.concat([customRenderer, customPopupMenu]);

export default class BpmnModelerComponent extends React.Component<BpmnModelerComponentProps> {

    private modeler!: Modeler;
    private elementRegistry!: BPMN.ElementRegistry;
    private bpmnFactory!: BPMN.BpmnFactory;
    private bpmnReplace!: BPMN.BpmnReplace;
    private divArea!: HTMLDivElement; 

    constructor(props: any) {
        super(props);
    }


    componentDidMount() {
        this.modeler = new CustomModeler({
            container: this.divArea,
            height: 1000,
            keyboard: {
                bindTo: document
            },
            additionalModules: [
                connectionIcons,
            ],
        });
        this.configureModules();
        this.elementRegistry = this.modeler.get<BPMN.ElementRegistry>('elementRegistry');
        this.bpmnFactory = this.modeler.get<BPMN.BpmnFactory>('bpmnFactory');
        this.bpmnReplace = this.modeler.get<BPMN.BpmnReplace>('bpmnReplace');
        this.modeler.on('element.dblclick', 1500, this.handleElementDoubleClick as (obj: BPMN.Event) => void);
        this.modeler.on('element.paste', 1500, this.handleElementPaste as (obj: BPMN.Event) => void);
        this.modeler.on('element.changed', 1500, this.handleElementChanged as (obj: BPMN.Event) => void);
        this.modeler.on('create.ended', 1500, this.handleCreateEnded as (obj: BPMN.Event) => void);
        this.modeler.on('shape.add', 1500, this.handleAddShapeOrConnection as (obj: BPMN.Event) => void);
        this.modeler.on('shape.remove', 1500, this.handleRemoveShape as (obj: BPMN.Event) => void);
        this.modeler.on('connection.add', 1500, this.handleAddShapeOrConnection as (obj: BPMN.Event) => void);
        this.modeler.on('label.add', 1500, () => this.lastPasted = undefined);
        this.modeler.importXML(this.props.diagramXML, this.handleOnModelError)
    }

    existsMainEntityTypeRelatedNodes(): boolean {

        var entities = this.props.entities;
        var result = false;
        this.elementRegistry.forEach(e => {

            var model = entities[e.id];

            if (!model)
                return;

            if (e.type == "bpmn:Lane" &&
                (model as WorkflowLaneModel).actorsEval != null)
                result = true;

            if (e.type == "bpmn:StartEvent" &&
                (model as WorkflowEventModel).task != null &&
                ((model as WorkflowEventModel).task!.action != null || (model as WorkflowEventModel).task!.condition != null))
                result = true;

            if (BpmnUtils.isConnection(e.type) &&
                ((model as WorkflowConnectionModel).action != null) ||
                ((e.type == "bpmn:ExclusiveGateway" || e.type == "bpmn:InclusiveGateway") && (model as WorkflowConnectionModel).condition != null))
                result = true;

            if (BpmnUtils.isTaskAnyKind(e.type) &&
                (model as WorkflowActivityModel).script != null)
                result = true;
        });

        return result;
    }

    private handleOnModelError = (err : string) => {
        if (err)
            throw new Error('Error rendering the model ' + err);
        else
            this.modeler.get<connectionIcons.ConnectionIcons>('connectionIcons').show();
    }

    configureModules() {
        var conIcons = this.modeler.get<connectionIcons.ConnectionIcons>('connectionIcons');
        conIcons.hasAction = con => {
            var mod = this.props.entities[con.id] as (WorkflowConnectionModel | undefined);
            return mod && mod.action || undefined;
        };

        conIcons.hasCondition = con => {
            var mod = this.props.entities[con.id] as (WorkflowConnectionModel | undefined);
            return mod && mod.condition || undefined;
        };

        var cusRenderer = this.modeler.get<customRenderer.CustomRenderer>('customRenderer');
        cusRenderer.getConnectionType = con => {
            var mod = this.props.entities[con.id] as (WorkflowConnectionModel | undefined);
            return mod && mod.type || undefined;
        }

        conIcons.show();
    }

    private saveXmlAsync(options: BPMN.SaveOptions): Promise<string> {
        return new Promise<string>((resolve, reject) => {
            this.modeler.saveXML(options, (err, xml) => {
                if (err)
                    return reject(err);
                else
                    return resolve(xml);
            })
        });
    }

    private saveSvgAsync(options: BPMN.SaveOptions): Promise<string> {
        return new Promise<string>((resolve, reject) => {
            this.modeler.saveSVG(options, (err, svgStr) => {
                if (err)
                    return reject(err);
                else
                    return resolve(svgStr);
            })
        });
    }

    getXml(): Promise<string> {
        return this.saveXmlAsync({ });
    }

    getSvg(): Promise<string> {
        return this.saveSvgAsync({ });
    }

    private fireElementChanged(element: Object) {

        this.modeler._emit('elements.changed', { elements: [element] });
    }
    

    newModel(element: BPMN.DiElement): ModelEntity {

        const mainEntityType = this.props.workflow.mainEntityType!;
        const elementType = element.type;
        const elementName = element.businessObject.name;

        if (elementType == "bpmn:Participant")
            return WorkflowPoolModel.New({
                name: elementName,
            });

        if (elementType == "bpmn:Lane")
            return WorkflowLaneModel.New({
                name: elementName,
                mainEntityType: mainEntityType,
            });

        if (elementType == "bpmn:StartEvent") {
            return WorkflowEventModel.New({
                name: elementName,
                type: "Start",
                mainEntityType: mainEntityType,
            });
        }

        if (elementType == "bpmn:BoundaryEvent") {
            var parentTask = this.props.entities[element.host.id] as WorkflowActivityModel;
            var boundaryTimer = WorkflowEventModel.New({
                name: elementName,
                type: "BoundaryInterruptingTimer",
                mainEntityType: mainEntityType,
                bpmnElementId: element.id,
                timer: WorkflowTimerEmbedded.New({
                    duration: TimeSpanEmbedded.New({ days: 1 }),
                })
            });

            parentTask.boundaryTimers.push(newMListElement(boundaryTimer));
            return boundaryTimer;
        }

        if (BpmnUtils.isTaskAnyKind(elementType))
            return WorkflowActivityModel.New({
                name: elementName,
                type: "Task",
                workflow: this.props.workflow,
                mainEntityType: mainEntityType,
            });

        if (elementType == "bpmn:IntermediateCatchEvent")
            return WorkflowEventModel.New({
                name: elementName,
                type: "IntermediateTimer",
                mainEntityType: mainEntityType,
                bpmnElementId: element.id,
                timer: WorkflowTimerEmbedded.New({
                    duration: TimeSpanEmbedded.New({ days: 1 }),
                }),
            });

        if (BpmnUtils.isConnection(elementType))
            return WorkflowConnectionModel.New({
                name: elementName,
                mainEntityType: mainEntityType,
                type: "Normal",
            });

        throw new Error("Impossible to create new Model: Unexpected " + elementType);
    }

    getModel(element: BPMN.DiElement): ModelEntity | undefined {

        if (element.type == "bpmn:BoundaryEvent") {
            var timers = (this.props.entities[element.host.id] as WorkflowActivityModel).boundaryTimers.singleOrNull(a => a.element.bpmnElementId == element.id);
            if (!timers)
                return undefined;

            return timers.element;
        }
        else
            return this.props.entities[element.id];
    }

    setModel(element: BPMN.DiElement, value: ModelEntity) {
        if (element.type == "bpmn:BoundaryEvent") {
            var parentTask = (this.props.entities[element.host.id] as WorkflowActivityModel);
            parentTask.boundaryTimers.single(a => a.element.bpmnElementId == element.id).element = (value as WorkflowEventModel);
        }
        else
            this.props.entities[element.id] = value;
    }

    handleElementDoubleClick = (e: BPMN.DoubleClickEvent) => {
        if (e.element.type == "bpmn:EndEvent")
            return;
         
        var model = this.getModel(e.element);

        if (!model) {
            if (BpmnUtils.isConnection(e.element.type)) {
                model = this.props.entities[e.element.id] = this.newModel(e.element);
            }
            else
                throw new Error("No Model found for " + e.element.id);
        }

        (model as any).name = e.element.businessObject.name;

        if (BpmnUtils.isConnection(e.element.type)) {
            var sourceElementType = (e.element.businessObject as BPMN.ConnectionModdleElemnet).sourceRef.$type;
            var connModel = (model as WorkflowConnectionModel);

            connModel.needCondition = (sourceElementType == "bpmn:ExclusiveGateway" || sourceElementType == "bpmn:InclusiveGateway");
            connModel.needOrder = sourceElementType == "bpmn:ExclusiveGateway";
        }

        e.preventDefault();
        e.stopPropagation();

        Navigator.view(model).then(me => {

            if (me) {
                this.setModel(e.element, me);

                e.element.businessObject.name = (me as any).name;

                if (BpmnUtils.isTaskAnyKind(e.element.type)) {
                    var dt = (me as WorkflowActivityModel).type;
                    e.element.type =
                        (dt == "CallWorkflow" || dt == "DecompositionWorkflow") ? "bpmn:CallActivity" :
                            dt == "Decision" ? "bpmn:UserTask" :
                                dt == "Script" ? "bpmn:ScriptTask" :
                                    "bpmn:Task";
                } else if (e.element.type == "bpmn:StartEvent") {
                    var et = (me as WorkflowEventModel).type;
                    e.element.type = (et == "Start" || et == "ScheduledStart") ? "bpmn:StartEvent" : "bpmn:EndEvent";

                    var bo = e.element.businessObject;
                    var shouldEvent =
                        (et == "Start" || et == "Finish") ? null :
                            (me as WorkflowEventModel).task!.triggeredOn == "Always" ? "bpmn:TimerEventDefinition" :
                                "bpmn:ConditionalEventDefinition";

                    if (shouldEvent) {
                        if (!bo.eventDefinitions)
                            bo.eventDefinitions = [];

                        bo.eventDefinitions.filter(a => a.$type != shouldEvent).forEach(a => bo.eventDefinitions!.remove(a));
                        if (bo.eventDefinitions.length == 0)
                            bo.eventDefinitions.push(this.bpmnFactory.create(shouldEvent, {}));
                    } else {
                        bo.eventDefinitions = undefined;
                    }
                }

                var newName = (me as any).name;

                if (WorkflowConnectionModel.isInstance(me)) {

                    if (newName)
                        newName = newName.tryBeforeLast(":") || newName;

                    if (me.order)
                        newName = newName + ": " + me.order;
                }

                this.modeler.get<any>("modeling").updateProperties(e.element, {
                    name: newName,
                });
            };
        }).done();
    }

    handleElementChanged = (e: BPMN.ElementEvent) => {

        if (BpmnUtils.isTaskAnyKind(e.element.type)) {
            const act = this.props.entities[e.element.id] as WorkflowActivityModel | undefined;
            if (act) {
                act.name = e.element.businessObject.name;
                act.modified = true;
            }
        }
        else if (e.element.type == "bpmn:BoundaryEvent") {
            if (e.element.host) {
                var timer = this.getModel(e.element);
                if (timer) {
                    (timer as WorkflowEventModel).type = (e.element.businessObject as any).cancelActivity ? "BoundaryInterruptingTimer" : "BoundaryForkTimer";
                    this.setModel(e.element, timer);
                }
            }
        }   
    }

    handleCreateEnded = (e: BPMN.EndedEvent) => {

        let shape = e.context.shape;
        const target = e.context.target;

        if (shape.type == "bpmn:EndEvent")
            return;

        this.isReplacing = true;
        try {
            if (shape.type == "bpmn:BoundaryEvent") {
                shape = this.bpmnReplace.replaceElement(shape, {
                    type: "bpmn:BoundaryEvent",
                    eventDefinitionType: "bpmn:TimerEventDefinition"
                });
            }
            else if (shape.type == "bpmn:IntermediateThrowEvent") {
                shape = this.bpmnReplace.replaceElement(shape, {
                    type: "bpmn:IntermediateCatchEvent",
                    eventDefinitionType: "bpmn:TimerEventDefinition"
                });
            }

            var model = this.newModel(shape);
            if (shape.type != "bpmn:BoundaryEvent")
                this.props.entities[shape.id!] = model;
        } finally
        {
            this.isReplacing = false;
        }
    }

    lastPasted?: { id: string; name?: string };
    handleElementPaste = (e: BPMN.PasteEvent) => {
        if (this.lastPasted) {
            console.error("lastPasted not consumed: " + this.lastPasted.id);
        }

        if (e.descriptor.type != "label")
            this.lastPasted = {
                id: e.descriptor.id,
                name: e.descriptor.name
            };
    }

    isReplacing: boolean = false;
    handleRemoveShape = (e: BPMN.ElementEvent) => {

        if (this.isReplacing)
            return;

        if (e.element.type == "bpmn:BoundaryEvent") {

            var parentActivity = Dic.getValues(this.props.entities)
                .single(model => WorkflowActivityModel.isInstance(model) &&
                    model.boundaryTimers.some(a => a.element.bpmnElementId == e.element.id)) as WorkflowActivityModel;

            var timer = parentActivity.boundaryTimers.single(a => a.element.bpmnElementId == e.element.id);
            parentActivity.boundaryTimers.remove(timer);
        }
        else {
            var model = this.props.entities[e.element.id];
            if (model && model.isNew)
                delete this.props.entities[e.element.id];
        };
    }

    handleAddShapeOrConnection = (e: BPMN.ElementEvent) => {
        if (this.lastPasted) {
            var model = this.props.entities[this.lastPasted.id];
            if (model) {
                var clone: ModelEntity = JSON.parse(JSON.stringify(model));
                if (WorkflowLaneModel.isInstance(clone))
                    clone.actors.forEach(a => a.rowId = null);

                this.props.entities[e.element.id] = clone ;
            }

            if (this.lastPasted.name)
                e.element.businessObject.name = this.lastPasted.name;

            this.lastPasted = undefined;
        }
    }

    componentWillUnmount() {
        this.modeler.destroy();
    }

    componentWillReceiveProps(nextProps: BpmnModelerComponentProps) {
        if (this.modeler) {
            if (nextProps.diagramXML !== undefined && this.props.diagramXML !== nextProps.diagramXML) {
                this.modeler.importXML(nextProps.diagramXML, this.handleOnModelError);
            }
        }
    }

    setDiv = (div: HTMLDivElement) => {
        if (this.divArea)
            this.divArea.removeEventListener("click", this.clickConnectionIconEvent as EventListener);

        this.divArea = div;

        if (this.divArea)
            this.divArea.addEventListener("click", this.clickConnectionIconEvent);
    }

    clickConnectionIconEvent = (e: MouseEvent) => {
        var d = e.target as HTMLDivElement;

        if (d.classList && d.classList.contains("connection-icon")) {
            const lite = parseLite(d.dataset["key"]!);
            Navigator.navigate(lite).done();
        }
    }

    handleZoomClick = (e: React.MouseEvent<any>) => {
        var zoomScroll = this.modeler.get<any>("zoomScroll");
        zoomScroll.reset();
    }

    render() {
        return (
            <div>
                <Button style={{ marginLeft: "20px" }} onClick={this.handleZoomClick}>{WorkflowMessage.ResetZoom.niceToString()}</Button>
                <div ref={this.setDiv} />
            </div>
        );
    }
}
