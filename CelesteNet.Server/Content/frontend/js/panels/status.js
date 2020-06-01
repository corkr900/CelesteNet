//@ts-check
import { rd, rdom, rd$, escape$, RDOMListHelper } from "../utils/rdom.js";
import mdcrd from "../utils/mdcrd.js";
import { FrontendBasicPanel } from "./basic.js";

/**
 * @typedef {import("material-components-web")} mdc
 */
/** @type {import("material-components-web")} */
const mdc = window["mdc"]; // mdc

export class FrontendStatusPanel extends FrontendBasicPanel {
  /**
   * @param {import("../frontend.js").Frontend} frontend
   */
  constructor(frontend) {
    super(frontend);
    this.header = "Status";

    this.data = {
      Alive: false,
      Connections: 0,
      Sessions: 0,
    };

    /** @type {[string | ((el: HTMLElement) => HTMLElement), () => void][] | [string | ((el: HTMLElement) => HTMLElement)][]} */
    this.list = [
      [
        el => rd$(el)`<span>
          <b>Control Panel Sync</b><br>
          ${this.frontend.sync.status || "init"}<br>
          <code>${this.frontend.sync.state || "invalid"}</code>
        </span>`
      ]

    ];

    for (let key in this.data) {
      // @ts-ignore
      this.list.push([
        el => rd$(el)`<span>
          <b>${key}</b>:${" " + this.data[key]}
        </span>`
      ]);
    }
  }

  async update() {
    const dataPrev = this.data;
    dataPrev.Alive = false;
    let data;

    try {
      data = await fetch("/status")
        .then(r => r.json());
      data.Alive = true;
    } catch (e) {
      console.error(e);
      if (data)
        data.Alive = false;
    }

    this.data = data || dataPrev;
  }

  render(el) {
    const state = this.frontend.sync.status;
    this.progress =
      (state.startsWith("closed") || state.startsWith("error")) ? 1 :
      (state.startsWith("connecting")) ? 2 :
      (state.startsWith("open")) ? 0 :
      -2;

    return super.render(el);
  }

}
