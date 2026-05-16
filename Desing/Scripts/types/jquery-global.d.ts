/**
 * Minimal global jQuery typings for legacy Razor inline scripts (no npm @types/jquery in repo).
 * Declaration merge extends `Window` for `window.jQuery`.
 */
interface JQuery {
  readonly length: number;
  on(events: string, handler: (this: HTMLElement, ev: Event) => void): this;
  val(): string | number | string[] | undefined;
  val(value: string): this;
}

interface JQueryStatic {
  (selector: string): JQuery;
  (callback: (this: Document, $: JQueryStatic) => void): unknown;
}

interface Window {
  jQuery?: JQueryStatic;
}
