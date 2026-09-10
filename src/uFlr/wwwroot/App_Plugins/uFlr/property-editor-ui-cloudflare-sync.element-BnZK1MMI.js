import { html as c, property as R, state as v, customElement as y } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as g } from "@umbraco-cms/backoffice/lit-element";
import { c as N } from "./client.gen-Crtm1jAh.js";
var r = /* @__PURE__ */ ((t) => (t.NOT_FOUND = "NotFound", t.UNKNOWN = "Unknown", t.PREPARING = "Preparing", t.ERRORED = "Errored", t.READY = "Ready", t))(r || {});
class P {
  static getStatus(e) {
    return ((e == null ? void 0 : e.client) ?? N).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/uflr/api/v1/status",
      ...e
    });
  }
}
var m = Object.defineProperty, U = Object.getOwnPropertyDescriptor, E = (t) => {
  throw TypeError(t);
}, o = (t, e, a, n) => {
  for (var s = n > 1 ? void 0 : n ? U(e, a) : e, d = t.length - 1, h; d >= 0; d--)
    (h = t[d]) && (s = (n ? h(e, a, s) : h(s)) || s);
  return n && s && m(e, a, s), s;
}, O = (t, e, a) => e.has(t) || E("Cannot " + a), l = (t, e, a) => (O(t, e, "read from private field"), a ? a.call(t) : e.get(t)), p = (t, e, a) => e.has(t) ? E("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, a), i, _, f;
let u = class extends g {
  constructor() {
    super(...arguments), this._value = null, this._status = null, this._statusLoading = !1, p(this, i, async () => {
      var e;
      if (!((e = this.value) != null && e.CloudflareAssetId)) {
        this._status = null;
        return;
      }
      this._statusLoading = !0;
      const { data: t } = await P.getStatus({ query: { assetId: this.value.CloudflareAssetId } });
      this._statusLoading = !1, this._status = t, (this._status === r.UNKNOWN || this._status === r.PREPARING) && setTimeout(() => l(this, i).call(this), 5e3);
    }), p(this, _, (t) => {
      switch (t) {
        case r.PREPARING:
          return "warning";
        case r.ERRORED:
          return "danger";
        case r.READY:
          return "positive";
        default:
          return "default";
      }
    }), p(this, f, (t) => {
      switch (t) {
        case r.PREPARING:
          return "Preparing";
        case r.ERRORED:
          return "Errored";
        case r.READY:
          return "Ready";
        case r.NOT_FOUND:
          return "Not Found";
        default:
          return "Unknown";
      }
    });
  }
  set value(t) {
    this._value = t, l(this, i).call(this);
  }
  get value() {
    return this._value;
  }
  render() {
    return this.value ? c`<uui-ref-node name=${this.value.CloudflareAssetId ?? "No asset id"} detail=${this.value.PlaybackUrl ?? "No playback URL"} readonly>
          <uui-icon slot="icon" name="icon-video"></uui-icon>
          ${this._statusLoading ? c`<uui-loader size="s" slot="tag"></uui-loader>` : c`<uui-tag size="s" slot="tag" color=${l(this, _).call(this, this._status)} @click=${l(this, i)}>${l(this, f).call(this, this._status)}</uui-tag>`}
        </uui-ref-node>` : c`<uui-tag look="placeholder">No video uploaded to Cloudflare</uui-tag>`;
  }
};
i = /* @__PURE__ */ new WeakMap();
_ = /* @__PURE__ */ new WeakMap();
f = /* @__PURE__ */ new WeakMap();
o([
  R()
], u.prototype, "value", 1);
o([
  v()
], u.prototype, "_value", 2);
o([
  v()
], u.prototype, "_status", 2);
o([
  v()
], u.prototype, "_statusLoading", 2);
u = o([
  y("uflr-property-editor-ui-cloudflare-sync")
], u);
const I = u;
export {
  u as UflrPropertyEditorUICloudflareSyncElement,
  I as default
};
//# sourceMappingURL=property-editor-ui-cloudflare-sync.element-BnZK1MMI.js.map
