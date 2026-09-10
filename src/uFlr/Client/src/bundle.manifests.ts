import { ManifestPropertyEditorSchema, ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor'

const entryPointManifest = {
  name: 'uFlr Entry Point',
  type: 'backofficeEntryPoint',
  alias: 'uFlr.EntryPoint',
  js: () => import('./entrypoint.js'),
}

const editorManifest: ManifestPropertyEditorUi = {
  type: 'propertyEditorUi',
  alias: 'uFlr.PropertyEditorUi.CloudflareSync',
  name: 'Cloudflare Sync',
  element: () => import('./property-editor-ui-cloudflare-sync.element.js'),
  meta: {
    label: 'Cloudflare Sync',
    icon: 'icon-video',
    group: 'media',
    propertyEditorSchemaAlias: 'uFlr.Sync',

    settings: {
      properties: [
        {
          alias: "uploadPropertyAlias",
          label: "Upload Property Alias",
          description: "Set the alias of the upload property editor to sync with",
          propertyEditorUiAlias: "Umb.PropertyEditorUi.TextBox",
        },
      ],
    },
  },
};

const schemaManifest: ManifestPropertyEditorSchema = {
  type: 'propertyEditorSchema',
  name: 'Cloudflare Sync',
  alias: 'uFlr.Sync',
  meta: {
    defaultPropertyEditorUiAlias: 'uFlr.PropertyEditorUi.CloudflareSync'
  },
};

export const manifests = [
  entryPointManifest,
  editorManifest,
  schemaManifest,
];
