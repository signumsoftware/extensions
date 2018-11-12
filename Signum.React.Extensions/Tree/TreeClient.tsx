import * as React from 'react'
import * as QueryString from 'query-string'
import { ajaxPost } from '../../../Framework/Signum.React/Scripts/Services';
import { EntitySettings } from '../../../Framework/Signum.React/Scripts/Navigator'
import * as Navigator from '../../../Framework/Signum.React/Scripts/Navigator'
import * as Finder from '../../../Framework/Signum.React/Scripts/Finder'
import { EntityOperationSettings } from '../../../Framework/Signum.React/Scripts/Operations'
import * as Operations from '../../../Framework/Signum.React/Scripts/Operations'
import * as EntityOperations from '../../../Framework/Signum.React/Scripts/Operations/EntityOperations'
import { Type } from '../../../Framework/Signum.React/Scripts/Reflection'
import { Lite } from '../../../Framework/Signum.React/Scripts/Signum.Entities'
import { TreeEntity, TreeOperation, MoveTreeModel, TreeMessage } from './Signum.Entities.Tree'
import TreeModal from './TreeModal'
import { FilterRequest, FilterOption } from "../../../Framework/Signum.React/Scripts/FindOptions";
import { ImportRoute } from "../../../Framework/Signum.React/Scripts/AsyncImport";
import { getAllTypes, getTypeInfo } from "../../../Framework/Signum.React/Scripts/Reflection";
import { TypeInfo } from "../../../Framework/Signum.React/Scripts/Reflection";
import TreeButton from './TreeButton'
import { toLite } from "../../../Framework/Signum.React/Scripts/Signum.Entities";
import { SearchControlLoaded } from "../../../Framework/Signum.React/Scripts/Search";
import { DisabledMixin } from "../Basics/Signum.Entities.Basics";

export function start(options: { routes: JSX.Element[] }) {
  options.routes.push(<ImportRoute path="~/tree/:typeName" onImportModule={() => import("./TreePage")} />);

  Navigator.addSettings(new EntitySettings(MoveTreeModel, e => import('./Templates/MoveTreeModel')));

  Operations.addSettings(
    new EntityOperationSettings(TreeOperation.CreateChild, { contextual: { isVisible: ctx => ctx.context.container instanceof SearchControlLoaded } }),
    new EntityOperationSettings(TreeOperation.CreateNextSibling, { contextual: { isVisible: ctx => ctx.context.container instanceof SearchControlLoaded } }),
    new EntityOperationSettings(TreeOperation.Move, {
      onClick: ctx => moveModal(toLite(ctx.entity)).then(m => m && ctx.defaultClick(m)),
      contextual: { onClick: ctx => moveModal(ctx.context.lites[0]).then(m => m && ctx.defaultContextualClick(m)) }
    }),
    new EntityOperationSettings(TreeOperation.Copy, {
      onClick: ctx => copyModal(toLite(ctx.entity)).then(m => {
        if (m) {
          ctx.onConstructFromSuccess = pack => EntityOperations.notifySuccess();
          ctx.defaultClick(m);
        }
      }),
      contextual: {
        onClick: ctx => copyModal(ctx.context.lites[0]).then(m => {
          if (m) {
            ctx.onConstructFromSuccess = pack => EntityOperations.notifySuccess();
            ctx.defaultContextualClick(m);
          }
        })
      }
    })
  );

  Finder.ButtonBarQuery.onButtonBarElements.push(ctx => {
    var ti = getTypeInfo(ctx.findOptions.queryKey);

    if (!ctx.searchControl.props.showBarExtension || ti == null || !isTree(ti))
      return undefined;

    return <TreeButton searchControl={ctx.searchControl} />;
  });
}



function moveModal(lite: Lite<TreeEntity>) {
  const s = settings[lite.EntityType];
  if (s && s.createMoveModel)
    return s.createMoveModel(lite, {});
  else
    return Navigator.view(MoveTreeModel.New({ insertPlace: "LastNode" }), {
      title: TreeMessage.Move0.niceToString(lite.toStr),
      modalSize: "md",
      extraComponentProps: { lite },
    })
}

function copyModal(lite: Lite<TreeEntity>) {
  const s = settings[lite.EntityType];
  if (s && s.createCopyModel)
    return s.createCopyModel(lite, {});
  else
    return Navigator.view(MoveTreeModel.New({ insertPlace: "LastNode" }), {
      title: TreeMessage.Copy0.niceToString(lite.toStr),
      modalSize: "md",
      extraComponentProps: { lite },
    });
}


export function treePath(typeName: string, filterOptions?: FilterOption[]): string {

  const query: any = {};
  if (filterOptions)
    Finder.Encoder.encodeFilters(query, filterOptions);

  return Navigator.history.createHref({ pathname: "~/tree/" + typeName, search: QueryString.stringify(query) });
}

export function hideSiblingsAndIsDisabled(ti: TypeInfo) {
  const type = new Type<TreeEntity>(ti.name);

  if (type.memberInfo(a => a.isSibling).notVisible == undefined)
    type.memberInfo(a => a.isSibling).notVisible = true;

  if (type.memberInfo(a => a.parentOrSibling).notVisible == undefined)
    type.memberInfo(a => a.parentOrSibling).notVisible = true;

  if (type.hasMixin(DisabledMixin)) {
    var mi = type.mixinMemberInfo(DisabledMixin, e => e.isDisabled);
    if (mi.notVisible == undefined)
      mi.notVisible = true;
  }
}

function getQuerySetting(typeName: string) {
  var qs = Finder.getSettings(typeName);
  if (!qs) {
    qs = { queryName: typeName };
    Finder.addSettings(qs);
  }
  return qs;
}

export function overrideOnFind(ti: TypeInfo) {
  var qs = getQuerySetting(ti.name);

  if (!qs.onFind)
    qs.onFind = (fo, mo) => openTree(ti.name, fo.filterOptions, { title: mo && mo.title });
}

export function overrideDefaultOrder(ti: TypeInfo) {
  var qs = getQuerySetting(ti.name);

  if (!qs.defaultOrderColumn) {
    qs.defaultOrderColumn = "FullName";
    qs.defaultOrderType = "Ascending";
  }
}

export function isTree(t: TypeInfo) {
  return (t.kind == "Entity" && t.operations && t.operations[TreeOperation.CreateNextSibling.key] != null) || false;
}

export function getAllTreeTypes() {
  return getAllTypes().filter(t => isTree(t));
}

export function openTree<T extends TreeEntity>(type: Type<T>, filterOptions?: FilterOption[], options?: { title?: string }): Promise<Lite<T> | undefined>;
export function openTree(typeName: string, filterOptions?: FilterOption[], options?: TreeModalOptions): Promise<Lite<TreeEntity> | undefined>;
export function openTree(type: Type<TreeEntity> | string, filterOptions?: FilterOption[], options?: TreeModalOptions): Promise<Lite<TreeEntity> | undefined> {
  const typeName = type instanceof Type ? type.typeName : type;

  return import("./TreeModal")
    .then((TM: { default: typeof TreeModal }) => TM.default.open(typeName, filterOptions || [], options));
}

export interface TreeModalOptions {
  title?: string;
  excludedNodes?: Array<Lite<TreeEntity>>;
}


export interface TreeSettings<T extends TreeEntity> {
  createCopyModel?: (from: T | Lite<T>, dropConfig: Partial<MoveTreeModel>) => Promise<MoveTreeModel | undefined>;
  createMoveModel?: (from: T | Lite<T>, dropConfig: Partial<MoveTreeModel>) => Promise<MoveTreeModel | undefined>;
  dragTargetIsValid?: (draggedNode: TreeNode, targetNode: TreeNode | null) => Promise<boolean>;
}

export const settings: {
  [typeName: string]: TreeSettings<any>;
} = {};

export function register<T extends TreeEntity>(type: Type<T>, setting: TreeSettings<T>) {
  settings[type.typeName] = setting;
}


export type TreeNodeState = "Collapsed" | "Expanded" | "Filtered" | "Leaf";

export interface TreeNode {
  lite: Lite<TreeEntity>;
  name: string;
  disabled: boolean;
  childrenCount: number;
  level: number;
  loadedChildren: Array<TreeNode>;
  nodeState: TreeNodeState;
}

export function fixState(node: TreeNode) {

  node.nodeState = node.childrenCount == 0 ? "Leaf" :
    node.loadedChildren.length == 0 ? "Collapsed" :
      node.childrenCount == node.loadedChildren.length ? "Expanded" : "Filtered";

  node.loadedChildren.forEach(n => fixState(n));
}

function fixNodes(nodes: Array<TreeNode>) {
  nodes.forEach(n => fixState(n));
  return nodes;
}


export namespace API {
  export function findNodes(typeName: string, request: FindNodesRequest): Promise<Array<TreeNode>> {
    return ajaxPost<Array<TreeNode>>({ url: `~/api/tree/findNodes/${typeName}` }, request).then(ns => fixNodes(ns));
  }

  export interface FindNodesRequest {
    userFilters: Array<FilterRequest>;
    frozenFilters: Array<FilterRequest>;
    expandedNodes: Array<Lite<TreeEntity>>;
  }
}
