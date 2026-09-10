const t = {
  name: "uFlr Entry Point",
  type: "backofficeEntryPoint",
  alias: "uFlr.EntryPoint",
  js: () => import("./entrypoint-Ds0YLQXJ.js")
}, r = {
  type: "propertyEditorUi",
  alias: "uFlr.PropertyEditorUi.CloudflareSync",
  name: "Cloudflare Sync",
  element: () => import("./property-editor-ui-cloudflare-sync.element-BnZK1MMI.js"),
  meta: {
    label: "Cloudflare Sync",
    icon: "icon-video",
    group: "media",
    propertyEditorSchemaAlias: "uFlr.Sync",
    settings: {
      properties: [
        {
          alias: "uploadPropertyAlias",
          label: "Upload Property Alias",
          description: "Set the alias of the upload property editor to sync with",
          propertyEditorUiAlias: "Umb.PropertyEditorUi.TextBox"
        }
      ]
    }
  }
}, e = {
  type: "propertyEditorSchema",
  name: "Cloudflare Sync",
  alias: "uFlr.Sync",
  meta: {
    defaultPropertyEditorUiAlias: "uFlr.PropertyEditorUi.CloudflareSync"
  }
}, o = [
  t,
  r,
  e
];
export {
  o as manifests
};
//# sourceMappingURL=u-gum.js.map
