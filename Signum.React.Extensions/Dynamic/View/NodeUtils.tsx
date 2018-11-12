﻿import * as React from 'react'
import { globalModules } from './GlobalModules'
import { ModifiableEntity, External } from '@framework/Signum.Entities'
import * as Navigator from '@framework/Navigator'
import { classes, Dic } from '@framework/Globals'
import { ViewReplacer } from '@framework/Frames/ReactVisitor'
import { Binding, EnumType, PropertyRoute, isTypeModifiableEntity } from '@framework/Reflection'
import { TypeContext } from '@framework/TypeContext'
import { EntityBaseProps } from '@framework/Lines/EntityBase'
import { DynamicViewValidationMessage } from '../Signum.Entities.Dynamic'
import { ExpressionOrValueComponent, FieldComponent } from './Designer'
import { FindOptionsLine, ViewNameComponent } from './FindOptionsComponent'
import { FindOptionsExpr, toFindOptions } from './FindOptionsExpression'
import { BaseNode, LineBaseNode, EntityBaseNode, EntityListBaseNode, EntityLineNode, ContainerNode, EntityTableColumnNode, CustomContextNode, TypeIsNode } from './Nodes'
import { toHtmlAttributes, HtmlAttributesExpression } from './HtmlAttributesExpression'
import { toStyleOptions, StyleOptionsExpression, subCtx } from './StyleOptionsExpression'
import { HtmlAttributesLine } from './HtmlAttributesComponent'
import { StyleOptionsLine } from './StyleOptionsComponent'
import TypeHelpComponent from '../../TypeHelp/TypeHelpComponent'
import { registeredCustomContexts } from '../DynamicViewClient'
import { findMany } from '@framework/Finder';
import * as Lines from '@framework/Lines'

export type ExpressionOrValue<T> = T | Expression<T>;

//ctx -> value
export type Expression<T> = { __code__: string };

export function isExpression(value: any): value is Expression<any> {
  return (value as Object).hasOwnProperty("__code__");
}

export interface NodeOptions<N extends BaseNode> {
  kind: string;
  group: "Container" | "Property" | "Collection" | "Search" | null;
  order: number | null;
  isContainer?: boolean;
  hasEntity?: boolean;
  hasCollection?: boolean;
  render: (node: DesignerNode<N>, parentCtx: TypeContext<ModifiableEntity>) => React.ReactElement<any> | undefined;
  renderCode?: (node: N, cc: CodeContext) => string;
  renderTreeNode: (node: DesignerNode<N>) => React.ReactElement<any>;
  renderDesigner: (node: DesignerNode<N>) => React.ReactElement<any>;
  validate?: (node: DesignerNode<N>, parentCtx: TypeContext<ModifiableEntity> | undefined) => string | null | undefined;
  validParent?: string;
  validChild?: string;
  avoidHighlight?: boolean;
  initialize?: (node: N, parentNode: DesignerNode<ContainerNode>) => void
}

export class CodeContext {
  ctxName: string;
  usedNames: string[];
  assignments: { [name: string]: string };
  imports: string[];

  constructor(ctxName: string, usedNames: string[], assignments: { [name: string]: string }, imports: string[]) {
    this.ctxName = ctxName;
    this.usedNames = usedNames;
    this.assignments = assignments;
    this.imports = imports;
  }


  subCtx(field?: string, options?: StyleOptionsExpression): CodeContext {
    if (!field && !options)
      return this;

    var newName = "ctx" + (this.usedNames.length + 1);

    return this.createNewContext(newName);
  }

  createNewContext(newName: string): CodeContext {
    this.usedNames.push(newName);
    return new CodeContext(newName, this.usedNames, this.assignments, this.imports);;
  }

  stringifyObject(expressionOrValue: ExpressionOrValue<any>): string {

    if (typeof expressionOrValue == "function")
      return expressionOrValue.toString();

    if (isExpression(expressionOrValue))
      return expressionOrValue.__code__;

    var result = JSON.stringify(expressionOrValue, (k, v) => {
      if (v != undefined && isExpression(v))
        return "%<%" + v.__code__ + "%>%";
      return v;
    }, 3);

    result = result.replace(/\"([^(\")"]+)\":/g, "$1:");
    result = result.replace(/"%<%(.*?)%>%"/g, s => {
      var bla = (JSON.parse(s) as string);
      return bla.substr(3, bla.length - 6);
    });
    return result;
  }

  elementCode(type: string, props: any, ...children: (string | undefined)[]) {

    var propsStr = props && Dic.map(props, (k, v) => v == undefined ? null :
      (k + "=" + (typeof (v) == "string" ? `"${v}"` : `{${this.stringifyObject(v)}}`)))
      .filter(a => a != null)
      .map(a => a!)
      .groupsOf(120, a => a.length)
      .map(gr => gr.join(" "))
      .join("    \r\n");

    if (children && children.length) {
      var childrenString = children.join("\n").indent(4);

      return (`<${type}${propsStr ? " " : ""}${propsStr || ""}>
${childrenString}
</${type}>`);

    } else {
      return (`<${type}${propsStr ? " " : ""}${propsStr || ""} />`);
    }
  }

  elementCodeWithChildren(type: string, props: any, node: ContainerNode): string {

    var childrensCode = node.children.map(c => renderCode(c, this));

    return this.elementCode(type, props, ...childrensCode);
  }

  elementCodeWithChildrenSubCtx(type: string, props: any, node: ContainerNode): string {

    var ctx = this.subCtx((node as any).field, (node as any).styleOptions);
    if (this != ctx)
      this.assignments[ctx.ctxName] = this.subCtxCode((node as any).field, (node as any).styleOptions).__code__;

    var childrensCode = node.children.map(c => renderCode(c, ctx));

    return this.elementCode(type, props, ...childrensCode);
  }

  subCtxCode(field?: string, options?: StyleOptionsExpression): Expression<any> {

    if (!field && !options)
      return { __code__: "ctx" };

    var propStr = field && "e => " + TypeHelpComponent.getExpression("e", field, "TypeScript", { stronglyTypedMixinTS: true });
    var optionsStr = options && this.stringifyObject(options);

    return { __code__: this.ctxName + ".subCtx(" + (propStr || "") + (propStr && optionsStr ? ", " : "") + (optionsStr || "") + ")" };
  }

  getEntityBasePropsEx(node: EntityBaseNode, options: { showAutoComplete?: boolean, findMany?: boolean, showMove?: boolean, avoidGetComponent?: boolean }): any/*: EntityBaseProps Expr*/ {

    var result: any /*EntityBaseProps*/ = {
      ctx: this.subCtxCode(node.field, node.styleOptions),
      labelText: node.labelText,
      labelHtmlAttributes: node.labelHtmlAttributes,
      formGroupHtmlAttributes: node.formGroupHtmlAttributes,
      visible: node.visible,
      readOnly: node.readOnly,
      create: node.create,
      onCreate: node.onCreate,
      remove: node.remove,
      onRemove: node.onRemove,
      find: node.find,
      ...(findMany ? { onFindMany: (node as EntityListBaseNode).onFindMany } : { onFind: node.onFind }),
      view: node.view,
      onView: node.onView,
      viewOnCreate: node.viewOnCreate,
      onChange: node.onChange,
      findOptions: node.findOptions,
      getComponent: options.avoidGetComponent == true ? undefined : this.getGetComponentEx(node, true),
      getViewPromise: toStringFunctionCode(node.viewName)
    };


    if (options.showAutoComplete)
      result.autoComplete = (node as EntityLineNode).autoComplete == undefined ? undefined :
        bindExpr(ac => ac == false ? null : undefined, (node as EntityLineNode).autoComplete);

    if (options.showMove)
      result.move = (node as EntityListBaseNode).move;

    return result;
  }

  getGetComponentEx(node: ContainerNode, withComment: boolean): Expression<any> | undefined {
    if (!node.children || !node.children.length)
      return undefined;

    var newName = "ctx" + (this.usedNames.length + 1);
    this.usedNames.push(newName);
    const cc = new CodeContext(newName, this.usedNames, {}, this.imports);

    const div = cc.elementCodeWithChildren("div", null, node);

    const assignments = Dic.map(cc.assignments, (k, v) => `const ${k} = ${v};`).join("\n");
    const block = !assignments ? `(${div})` : `{
${assignments.indent(4)}
    return (${div});
}`

    if (withComment)
      return { __code__: `(${cc.ctxName} /*: YourEntity*/) => ${block}` };
    else
      return { __code__: `${cc.ctxName} => ${block}` };
  }

}

export function toStringFunction(val: string | undefined | ((e: ModifiableEntity) => string)): undefined | ((e: ModifiableEntity) => string) {
  if (!val)
    return undefined;

  if (typeof val == "function")
    return val;

  return () => val;
}

export function toStringFunctionCode(val: ExpressionOrValue<string | ((e: ModifiableEntity) => string) | undefined>): Expression<((e: ModifiableEntity) => string)> | undefined {
  if (!val)
    return undefined;

  if (isExpression(val))
    return val;

  return { __code__: "mod => '" + val + "'" };
}


export interface DesignerContext {
  refreshView: () => void;
  onClose: () => void;
  getSelectedNode: () => DesignerNode<BaseNode> | undefined;
  setSelectedNode: (newSelectedNode: DesignerNode<BaseNode>) => void;
}

export class DesignerNode<N extends BaseNode> {
  parent?: DesignerNode<BaseNode>;
  context: DesignerContext;
  node: N;
  route?: PropertyRoute;

  constructor(parent: DesignerNode<BaseNode> | undefined, context: DesignerContext, node: N, route: PropertyRoute | undefined) {
    this.parent = parent;
    this.context = context;
    this.node = node;
    this.route = route;
  }

  static zero<N extends BaseNode>(context: DesignerContext, typeName: string) {
    var res = new DesignerNode(undefined, context, null as any as N, PropertyRoute.root(typeName));
    return res;
  }

  createChild<T extends BaseNode>(node: T): DesignerNode<T> {
    var route = this.fixRoute()
    const lbn = node as any as { field: string };
    if (lbn.field && route)
      route = route.tryAddMember("Member", lbn.field);

    var res = new DesignerNode<T>(this, this.context, node, route);

    return res;
  }

  reCreateNode(): DesignerNode<N> {
    if (this.parent == undefined)
      return this;

    return this.parent.createChild<N>(this.node);
  }

  fixRoute(): PropertyRoute | undefined {
    let res = this.route;

    if (!res)
      return undefined;

    if (this.node == undefined)
      return res;

    const options = registeredNodes[this.node.kind];
    if (options.kind == "CustomContext") {
      var cc = registeredCustomContexts[(this.node as BaseNode as CustomContextNode).typeContext];
      return cc.getPropertyRoute(this as DesignerNode<BaseNode> as DesignerNode<CustomContextNode>);
    }

    if (options.kind == "TypeIs") {
      var typeName = (this.node as BaseNode as TypeIsNode).typeName;
      if (typeName)
        return PropertyRoute.root(typeName);
    }

    if (options.hasCollection)
      res = res.tryAddMember("Indexer", "");

    if (!res)
      return undefined;

    if (options.hasEntity) {
      const tr = res.typeReference();
      if (tr.isLite)
        res = res.tryAddMember("Member", "Entity");
    }
    return res;
  }
}

export const registeredNodes: { [nodeType: string]: NodeOptions<BaseNode> } = {};

export function register<T extends BaseNode>(options: NodeOptions<T>) {
  registeredNodes[options.kind] = options as NodeOptions<BaseNode>;
}

export function treeNodeKind(dn: DesignerNode<BaseNode>) {
  return <small>{dn.node.kind}</small>;
}

export function treeNodeKindField(dn: DesignerNode<LineBaseNode>) {
  return <span><small>{dn.node.kind}:</small> <strong>{dn.node.field}</strong></span>;
}

export function treeNodeTableColumnProperty(dn: DesignerNode<EntityTableColumnNode>) {
  return <span><small>ETColumn:</small> <strong>{dn.node.property}</strong></span>;
}


export function renderWithViewOverrides(dn: DesignerNode<BaseNode>, parentCtx: TypeContext<ModifiableEntity>, vos: Navigator.ViewOverride<ModifiableEntity>[]) {

  var result = render(dn, parentCtx);
  if (result == null)
    return null;

  const es = Navigator.getSettings(parentCtx.propertyRoute.typeReference().name);
  if (vos.length) {
    const replacer = new ViewReplacer(result, parentCtx);
    vos.forEach(vo => vo.override(replacer));
    return replacer.result;
  } else {
    return result;
  }
}


export function renderCode(node: BaseNode, cc: CodeContext) {

  try {
    var no = registeredNodes[node.kind];

    var result = no.renderCode!(node, cc);

    if (node.visible)
      return `{ ${cc.stringifyObject(node.visible)} && ${result}}`

    return result;

  } catch (e) {
    return `/*ERROR ${(e as Error).message}*/`;
  }
}

export function render(dn: DesignerNode<BaseNode>, parentCtx: TypeContext<ModifiableEntity>) {
  try {
    if (evaluateAndValidate(parentCtx, dn.node, n => n.visible, isBooleanOrNull) == false)
      return null;

    const error = validate(dn, parentCtx);
    if (error)
      return (<div className="alert alert-danger">{getErrorTitle(dn)} {error}</div>);

    const sn = dn.context.getSelectedNode();

    if (sn && sn.node == dn.node && registeredNodes[sn.node.kind].avoidHighlight != true)
      return (
        <div style={{ border: "1px solid #337ab7", borderRadius: "2px" }}>
          {registeredNodes[dn.node.kind].render(dn, parentCtx)}
        </div>);

    return registeredNodes[dn.node.kind].render(dn, parentCtx);

  } catch (e) {
    return (<div className="alert alert-danger">{getErrorTitle(dn)}&nbsp;{(e as Error).message}</div>);
  }
}

export function getErrorTitle(dn: DesignerNode<BaseNode>) {
  const lbn = dn.node as LineBaseNode;
  if (lbn.field)
    return <strong>{dn.node.kind} ({lbn.field})</strong>;
  else
    return <strong>{dn.node.kind}</strong>;
}

export function renderDesigner(dn: DesignerNode<BaseNode>) {
  return (
    <div>
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, a => a.visible)} type="boolean" defaultValue={true} />
      {registeredNodes[dn.node.kind].renderDesigner(dn)}
    </div>
  );
}

export function asFunction(thisObject: React.Component<any, any>, expression: Expression<any>, getFieldName: () => string): (e: TypeContext<ModifiableEntity>) => any {

  const code = "ctx => " + expression.__code__;

  try {
    return evalWithScope.call(thisObject, code, globalModules);
  } catch (e) {
    throw new Error("Syntax in '" + getFieldName() + "':\r\n" + code + "\r\n" + (e as Error).message);
  }
}

export function evalWithScope(code: string, modules: any) {

  // Lines
  var ValueLine = Lines.ValueLine;

  return eval(code);
}

export function asFieldFunction(field: string): (e: ModifiableEntity) => any {
  const fixedRoute = TypeHelpComponent.getExpression("e", field, "TypeScript");

  const code = "(function(e){ return " + fixedRoute + ";})";

  try {
    return eval(code);
  } catch (e) {
    throw new Error("Syntax in '" + fixedRoute + "':\r\n" + code + "\r\n" + (e as Error).message);
  }
}

export function evaluate<F, T>(parentCtx: TypeContext<ModifiableEntity>, object: F, fieldAccessor: (from: F) => ExpressionOrValue<T> | undefined): T | undefined {

  return evaluateUntyped(parentCtx, fieldAccessor(object), () => Binding.getSingleMember(fieldAccessor));
}

export function evaluateUntyped(parentCtx: TypeContext<ModifiableEntity>, expressionOrValue: ExpressionOrValue<any> | undefined, getFieldName: () => string): any {
  if (expressionOrValue == null)
    return undefined;

  if (!isExpression(expressionOrValue))
    return expressionOrValue as any;

  if (!expressionOrValue.__code__)
    return undefined;

  var f = asFunction(parentCtx.frame!.entityComponent!, expressionOrValue, getFieldName);

  try {
    return f(parentCtx);
  } catch (e) {
    throw new Error("Eval '" + getFieldName() + "':\r\n" + (e as Error).message);
  }
}

export function evaluateAndValidate<F, T>(parentCtx: TypeContext<ModifiableEntity>, object: F, fieldAccessor: (from: F) => ExpressionOrValue<T>, validate: (val: any) => string | null) {
  var result = evaluate(parentCtx, object, fieldAccessor);

  var error = validate(result);
  if (error)
    throw new Error("Result '" + Binding.getSingleMember(fieldAccessor) + "':\r\n" + error);

  if (result == null)
    return undefined;

  return result;
}

export function validate(dn: DesignerNode<BaseNode>, parentCtx: TypeContext<ModifiableEntity> | undefined) {
  const options = registeredNodes[dn.node.kind];
  if (options.isContainer && options.validChild && (dn.node as ContainerNode).children && (dn.node as ContainerNode).children.some(c => c.kind != options.validChild))
    return DynamicViewValidationMessage.OnlyChildNodesOfType0Allowed.niceToString(options.validChild);

  if (options.validate)
    return options.validate(dn, parentCtx);

  return undefined;
}

export function isString(val: any) {
  return typeof val == "string" ? null : `The returned value (${JSON.stringify(val)}) should be a string`;
}

export function isNumber(val: any) {
  return typeof val == "number" ? null : `The returned value (${JSON.stringify(val)}) should be a number`;
}

export function isBoolean(val: any) {
  return typeof val == "boolean" ? null : `The returned value (${JSON.stringify(val)}) should be a boolean`;
}

export function isBooleanOrFunction(val: any) {
  return (typeof val == "boolean" || typeof val == "function") ? null : `The returned value (${JSON.stringify(val)}) should be a boolean or function`;
}

export function isFindOptions(val: any) {
  return typeof val == "object" ? null : `The returned value (${JSON.stringify(val)}) should be a valid findOptions`;
}

export function isStringOrNull(val: any) {
  return val == null || typeof val == "string" ? null : `The returned value (${JSON.stringify(val)}) should be a string or null`;
}

export function isEnum(val: any, enumType: EnumType<any>) {
  return val != null && typeof val == "string" && enumType.values().contains(val) ? null : `The returned value (${JSON.stringify(val)}) should be a valid ${enumType.type} (like ${enumType.values().joinComma(" or ")})`;
}

export function isEnumOrNull(val: any, enumType: EnumType<any>) {
  return val == null || typeof val == "string" && enumType.values().contains(val) ? null : `The returned value (${JSON.stringify(val)}) should be a valid ${enumType.type} (like ${enumType.values().joinComma(" or ")}) or null`;
}

export function isObject(val: any) {
  return val != null && typeof val == "object" ? null : `The returned value (${JSON.stringify(val)}) should be an object`;
}

export function isInList(val: any, values: string[]) {
  return val != null && typeof val == "string" && values.contains(val) ? null : `The returned value (${JSON.stringify(val)}) should be a value like ${values.joinComma(" or ")}`;
}

export function isInListOrNull(val: any, values: string[]) {
  return val == null || typeof val == "string" && values.contains(val) ? null : `The returned value (${JSON.stringify(val)}) should be a value like ${values.joinComma(" or ")} or null`;
}

export function isNumberOrNull(val: any) {
  return val == null || typeof val == "number" ? null : `The returned value (${JSON.stringify(val)}) should be a number or null`;
}

export function isNumberOrStringOrNull(val: any) {
  return val == null || typeof val == "number" || typeof val == "string" ? null : `The returned value (${JSON.stringify(val)}) should be a number or string or null`;
}

export function isBooleanOrNull(val: any) {
  return val == null || typeof val == "boolean" ? null : `The returned value (${JSON.stringify(val)}) should be a boolean or null`;
}

export function isBooleanOrFunctionOrNull(val: any) {
  return val == null || typeof val == "boolean" || typeof val == "function" ? null : `The returned value (${JSON.stringify(val)}) should be a boolean or function or null`;
}

export function isFunctionOrNull(val: any) {
  return val == null || typeof val == "function" ? null : `The returned value (${JSON.stringify(val)}) should be a function or null`;
}

export function isFunctionOrStringOrNull(val: any) {
  return val == null || typeof val == "function" || typeof val == "string" ? null : `The returned value (${JSON.stringify(val)}) should be a function or string or null`;
}

export function isArrayOrNull(val: any) {
  return val == null || Array.isArray(val) ? null : `The returned value (${JSON.stringify(val)}) should be an array or null`;
}

export function isFindOptionsOrNull(val: any) {
  return val == null || isFindOptions(val) == null ? null : `The returned value (${JSON.stringify(val)}) should be a findOptions or null`;
}

export function withChildrensSubCtx(dn: DesignerNode<ContainerNode>, parentCtx: TypeContext<ModifiableEntity>, element: React.ReactElement<any>) {
  var ctx = subCtx(parentCtx, (dn.node as any).field, (dn.node as any).styleOptions);
  return withChildrens(dn, ctx, element);
}

export function withChildrens(dn: DesignerNode<ContainerNode>, ctx: TypeContext<ModifiableEntity>, element: React.ReactElement<any>) {
  var nodes = dn.node.children && dn.node.children.map(n => render(dn.createChild(n), ctx)).filter(a => a != null).map(a => a!);
  return React.cloneElement(element, undefined, ...nodes);
}

export function mandatory<T extends BaseNode>(dn: DesignerNode<T>, fieldAccessor: (from: T) => any) {
  if (!fieldAccessor(dn.node))
    return DynamicViewValidationMessage.Member0IsMandatoryFor1.niceToString(Binding.getSingleMember(fieldAccessor), dn.node.kind);

  return undefined;
}

export function validateFieldMandatory(dn: DesignerNode<LineBaseNode>): string | undefined {
  return mandatory(dn, n => n.field) || validateField(dn);
}

export function validateEntityBase(dn: DesignerNode<EntityBaseNode>, parentCtx: TypeContext<ModifiableEntity> | undefined): string | undefined {
  return validateFieldMandatory(dn) ||
    (dn.node.findOptions && validateFindOptions(dn.node.findOptions, parentCtx)) ||
    viewNameOrChildrens(dn);
}


export function viewNameOrChildrens(dn: DesignerNode<EntityBaseNode>): string | undefined {
  if (dn.node.children && dn.node.children.length > 0 && dn.node.viewName != null)
    return DynamicViewValidationMessage.ViewNameIsNotAllowedWhileHavingChildren.niceToString()
}


export function validateField(dn: DesignerNode<LineBaseNode>): string | undefined {

  const parentRoute = dn.parent!.fixRoute();

  if (parentRoute == undefined)
    return undefined;

  const m = parentRoute.subMembers()[dn.node.field!]

  if (!m)
    return DynamicViewValidationMessage.Type0DoesNotContainsField1.niceToString(parentRoute.typeReference().name, dn.node.field);

  const options = registeredNodes[dn.node.kind]

  const isEntity = isTypeModifiableEntity(m.type);

  const DVVM = DynamicViewValidationMessage;

  if ((isEntity || false) != (options.hasEntity || false) ||
    (m.type.isCollection || false) != (options.hasCollection || false))
    return DVVM._0RequiresA1.niceToString(dn.node.kind,
      (options.hasEntity ?
        (options.hasCollection ? DVVM.CollectionOfEntities : DVVM.Entity) :
        (options.hasCollection ? DVVM.CollectionOfEnums : DVVM.Value)).niceToString());


  return undefined;
}

export function validateFindOptions(foe: FindOptionsExpr, parentCtx: TypeContext<ModifiableEntity> | undefined): string | undefined {
  if (!foe.queryName)
    return DynamicViewValidationMessage._0RequiresA1.niceToString("findOptions", "queryKey");

  return undefined;
}


let aggregates = ["Count", "Average", "Min", "Max", "Sum"];

export function validateAggregate(token: string) {
  var lastPart = token.tryAfterLast(".") || token;

  if (!aggregates.contains(lastPart))
    return DynamicViewValidationMessage.AggregateIsMandatoryFor01.niceToString("valueToken", aggregates.joinComma(External.CollectionMessage.Or.niceToString()));

  return undefined;
}

export function addBreakLines(breakLines: boolean, message: string): React.ReactNode[] {
  if (!breakLines)
    return [message];

  return message.split("\n").flatMap((e, i) => i == 0 ? [e] : [<br />, e]);
}

export function getEntityBaseProps(dn: DesignerNode<EntityBaseNode>, parentCtx: TypeContext<ModifiableEntity>, options: { showAutoComplete?: boolean, findMany?: boolean, showMove?: boolean, avoidGetComponent?: boolean, isEntityLine?: boolean }): EntityBaseProps {

  var result: EntityBaseProps = {
    ctx: parentCtx.subCtx(dn.node.field, toStyleOptions(parentCtx, dn.node.styleOptions)),
    labelText: evaluateAndValidate(parentCtx, dn.node, n => n.labelText, isStringOrNull),
    labelHtmlAttributes: toHtmlAttributes(parentCtx, dn.node.labelHtmlAttributes),
    formGroupHtmlAttributes: toHtmlAttributes(parentCtx, dn.node.formGroupHtmlAttributes),
    ...(options.isEntityLine ?
      { itemHtmlAttributes: toHtmlAttributes(parentCtx, (dn.node as EntityLineNode).itemHtmlAttributes) }
      : undefined),
    visible: evaluateAndValidate(parentCtx, dn.node, n => n.visible, isBooleanOrNull),
    readOnly: evaluateAndValidate(parentCtx, dn.node, n => n.readOnly, isBooleanOrNull),
    create: evaluateAndValidate(parentCtx, dn.node, n => n.create, isBooleanOrNull),
    onCreate: evaluateAndValidate(parentCtx, dn.node, n => n.onCreate, isFunctionOrNull),
    remove: evaluateAndValidate(parentCtx, dn.node, n => n.remove, isBooleanOrFunctionOrNull),
    onRemove: evaluateAndValidate(parentCtx, dn.node, n => n.onRemove, isFunctionOrNull),
    find: evaluateAndValidate(parentCtx, dn.node, n => n.find, isBooleanOrNull),
    ...(options.findMany ?
      { onFindMany: evaluateAndValidate(parentCtx, dn.node, (n: EntityListBaseNode) => n.onFindMany, isFunctionOrNull) } as any :
      { onFind: evaluateAndValidate(parentCtx, dn.node, n => n.onFind, isFunctionOrNull) }
    ),
    view: evaluateAndValidate(parentCtx, dn.node, n => n.view, isBooleanOrFunctionOrNull),
    onView: evaluateAndValidate(parentCtx, dn.node, n => n.onView, isFunctionOrNull),
    viewOnCreate: evaluateAndValidate(parentCtx, dn.node, n => n.viewOnCreate, isBooleanOrNull),
    onChange: evaluateAndValidate(parentCtx, dn.node, n => n.onChange, isFunctionOrNull),
    findOptions: dn.node.findOptions && toFindOptions(parentCtx, dn.node.findOptions),
    getComponent: options.avoidGetComponent == true ? undefined : getGetComponent(dn),
    getViewPromise: toStringFunction(evaluateAndValidate(parentCtx, dn.node, n => n.viewName, isStringOrNull))
  };

  if (options.showAutoComplete)
    (result as any).autoComplete = evaluateAndValidate(parentCtx, dn.node, n => (n as EntityLineNode).autoComplete, isBooleanOrNull) == false ? null : undefined;

  if (options.showMove)
    (result as any).move = evaluateAndValidate(parentCtx, dn.node, (n: EntityListBaseNode) => n.move, isBooleanOrFunctionOrNull);

  return result;
}




export function getGetComponent(dn: DesignerNode<ContainerNode>) {
  if (!dn.node.children || !dn.node.children.length)
    return undefined;

  return (ctxe: TypeContext<ModifiableEntity>) => withChildrens(dn, ctxe, <div />);
}

export function designEntityBase(dn: DesignerNode<EntityBaseNode>, options: { showAutoComplete?: boolean, findMany?: boolean, showMove?: boolean, isEntityLine?: boolean }) {

  const m = dn.route && dn.route.member;

  const typeName = m ? m.type.name : "YourEntity";

  return (
    <div>
      <FieldComponent dn={dn} binding={Binding.create(dn.node, n => n.field)} />
      <StyleOptionsLine dn={dn} binding={Binding.create(dn.node, n => n.styleOptions)} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.labelText)} type="string" defaultValue={m && m.niceName || ""} />
      <HtmlAttributesLine dn={dn} binding={Binding.create(dn.node, n => n.labelHtmlAttributes)} />
      <HtmlAttributesLine dn={dn} binding={Binding.create(dn.node, n => n.formGroupHtmlAttributes)} />
      {options.isEntityLine && <HtmlAttributesLine dn={dn} binding={Binding.create(dn.node, n => (n as EntityLineNode).itemHtmlAttributes)} />}
      <ViewNameComponent dn={dn} binding={Binding.create(dn.node, n => n.viewName)} typeName={m ? m.type.name : undefined} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.readOnly)} type="boolean" defaultValue={null} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.create)} type="boolean" defaultValue={null} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.onCreate)} type={null} defaultValue={null} exampleExpression={"() => Promise.resolve(modules.Reflection.New('" + typeName + "', { name: ''}))"} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.remove)} type="boolean" defaultValue={null} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.onRemove)} type={null} defaultValue={null} exampleExpression={"() => Promise.resolve(true)"} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.find)} type="boolean" defaultValue={null} />
      {!options.findMany && <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.onFind)} type={null} defaultValue={null} exampleExpression={"e => modules.Finder.find('" + typeName + "')"} />}
      {options.findMany && <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, (n: EntityListBaseNode) => n.onFindMany)} type={null} defaultValue={null} exampleExpression={"e => modules.Finder.findMany('" + typeName + "')"} />}
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.view)} type="boolean" defaultValue={null} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.onView)} type={null} defaultValue={null} exampleExpression={"e => modules.Navigator.view(e)"} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.viewOnCreate)} type="boolean" defaultValue={null} />
      {options.showMove && <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, (n: EntityListBaseNode) => n.move)} type="boolean" defaultValue={null} />}
      {options.showAutoComplete && <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => (n as EntityLineNode).autoComplete)} type="boolean" defaultValue={null} />}
      <FindOptionsLine dn={dn} binding={Binding.create(dn.node, n => n.findOptions)} avoidSuggestion={true} />
      <ExpressionOrValueComponent dn={dn} binding={Binding.create(dn.node, n => n.onChange)} type={null} defaultValue={null} exampleExpression={"() => this.forceUpdate()"} />
    </div>
  );
}



export function withClassNameEx(attrs: HtmlAttributesExpression | undefined, className: ExpressionOrValue<string>): HtmlAttributesExpression {
  if (attrs == undefined)
    return { className: className };

  attrs["className"] = bindExpr((c, a) => classes(c, a), className, attrs["className"]);

  return attrs;
}


export function toCodeEx(expr: ExpressionOrValue<string>) {
  return isExpression(expr) ? "(" + expr.__code__ + ")" :
    expr == null ? "null" :
      ("\"" + expr + "\"");
}

var lambdaBody = /^function\s*\(\s*([$a-zA-Z_][0-9a-zA-Z_$]*(\s*,\s*[$a-zA-Z_][0-9a-zA-Z_$]*\s*)*)\s*\)\s*{\s*(\"use strict\"\;)?\s*return\s*([^;]*)\s*;?\s*}$/
export function bindExpr(lambda: (...params: any[]) => any, ...parameters: ExpressionOrValue<any>[]): ExpressionOrValue<any> {
  if (parameters.every(a => a == null || !isExpression(a)))
    return lambda(...parameters);


  var parts = lambda.toString().match(lambdaBody)!;
  var params = parts[1].split(",").map(a => a.trim());
  var body = parts[4];

  var newBody = body.replace(/\b[$a-zA-Z_][0-9a-zA-Z_$]*\b/g, str => params.contains(str) ? toCodeEx(parameters[params.indexOf(str)]) : str);

  return {
    __code__: newBody
  }

}
