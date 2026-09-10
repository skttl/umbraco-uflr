import { html, customElement, property, state } from '@umbraco-cms/backoffice/external/lit';
import type { UmbPropertyEditorUiElement } from '@umbraco-cms/backoffice/property-editor';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { CloudflareValue } from './types/cloudflarevalue';
import { AssetStatus, Uflr } from './api';

@customElement('uflr-property-editor-ui-cloudflare-sync')
export class UflrPropertyEditorUICloudflareSyncElement extends UmbLitElement implements UmbPropertyEditorUiElement {
  @property() public set value(newValue: CloudflareValue | undefined | null) { this._value = newValue; this.#fetchStatus(); }
  public get value(): CloudflareValue | undefined | null { return this._value; }
  @state() private _value: CloudflareValue | undefined | null = null;
  @state() private _status: AssetStatus | null = null;
  @state() private _statusLoading = false;

  #fetchStatus = async () => {
    if (!this.value?.CloudflareAssetId) { this._status = null; return; }
    this._statusLoading = true;
    const { data } = await Uflr.getStatus({ query: { assetId: this.value.CloudflareAssetId } });
    this._statusLoading = false;
    this._status = data as unknown as AssetStatus;
    if (this._status === AssetStatus.UNKNOWN || this._status === AssetStatus.PREPARING) setTimeout(() => this.#fetchStatus(), 5000);
  }

  #assetStatusColor = (status: AssetStatus | null) => {
    switch (status) {
      case AssetStatus.PREPARING: return 'warning';
      case AssetStatus.ERRORED: return 'danger';
      case AssetStatus.READY: return 'positive';
      default: return 'default';
    }
  };

  #assetStatusLabel = (status: AssetStatus | null) => {
    switch (status) {
      case AssetStatus.PREPARING: return 'Preparing';
      case AssetStatus.ERRORED: return 'Errored';
      case AssetStatus.READY: return 'Ready';
      case AssetStatus.NOT_FOUND: return 'Not Found';
      default: return 'Unknown';
    }
  };

  override render() {
    return this.value
      ? html`<uui-ref-node name=${this.value.CloudflareAssetId ?? 'No asset id'} detail=${this.value.PlaybackUrl ?? 'No playback URL'} readonly>
          <uui-icon slot="icon" name="icon-video"></uui-icon>
          ${this._statusLoading ? html`<uui-loader size="s" slot="tag"></uui-loader>` : html`<uui-tag size="s" slot="tag" color=${this.#assetStatusColor(this._status)} @click=${this.#fetchStatus}>${this.#assetStatusLabel(this._status)}</uui-tag>`}
        </uui-ref-node>`
      : html`<uui-tag look="placeholder">No video uploaded to Cloudflare</uui-tag>`;
  }
}

export default UflrPropertyEditorUICloudflareSyncElement;
declare global { interface HTMLElementTagNameMap { 'uflr-property-editor-ui-cloudflare-sync': UflrPropertyEditorUICloudflareSyncElement; } }
