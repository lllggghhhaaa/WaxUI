const states = /* @__PURE__ */ new WeakMap();
const VIEWPORT_MARGIN = 8;
const HOVER_CLOSE_DELAY = 120;
const ARROW = 8;
function on(target, type, handler, removers, capture = false) {
  const listener = (e) => handler(e);
  target.addEventListener(type, listener, capture);
  removers.push(() => target.removeEventListener(type, listener, capture));
}
function clamp(value, lo, hi) {
  return Math.min(Math.max(value, lo), Math.max(lo, hi));
}
function cancelCloseTimer(state) {
  if (state.closeTimer !== null) {
    clearTimeout(state.closeTimer);
    state.closeTimer = null;
  }
}
function scheduleClose(state) {
  if (state.closeTimer !== null) return;
  state.closeTimer = window.setTimeout(() => {
    state.closeTimer = null;
    void state.dotnet.invokeMethodAsync("OnWaxPopoverTriggerLeave");
  }, HOVER_CLOSE_DELAY);
}
function leavesPopover(state, related) {
  if (!(related instanceof Node)) return true;
  return !state.panel.contains(related) && !state.triggerEl.contains(related);
}
function bindTrigger(state) {
  for (const remove of state.triggerRemovers) remove();
  state.triggerRemovers = [];
  const { triggerEl, panel, dotnet } = state;
  switch (state.opts.trigger) {
    case "click":
      on(triggerEl, "click", () => void dotnet.invokeMethodAsync("OnWaxPopoverTriggerActivate"), state.triggerRemovers);
      break;
    case "hover":
      on(triggerEl, "mouseenter", () => {
        cancelCloseTimer(state);
        void dotnet.invokeMethodAsync("OnWaxPopoverTriggerEnter");
      }, state.triggerRemovers);
      on(triggerEl, "mouseleave", () => scheduleClose(state), state.triggerRemovers);
      on(triggerEl, "focusin", () => {
        cancelCloseTimer(state);
        void dotnet.invokeMethodAsync("OnWaxPopoverTriggerFocus");
      }, state.triggerRemovers);
      on(triggerEl, "focusout", (e) => {
        if (leavesPopover(state, e.relatedTarget)) scheduleClose(state);
      }, state.triggerRemovers);
      on(panel, "mouseenter", () => cancelCloseTimer(state), state.triggerRemovers);
      on(panel, "mouseleave", () => scheduleClose(state), state.triggerRemovers);
      break;
    case "focus":
      on(triggerEl, "focusin", () => void dotnet.invokeMethodAsync("OnWaxPopoverTriggerFocus"), state.triggerRemovers);
      on(triggerEl, "focusout", (e) => {
        if (leavesPopover(state, e.relatedTarget)) {
          void dotnet.invokeMethodAsync("OnWaxPopoverTriggerBlur");
        }
      }, state.triggerRemovers);
      break;
  }
}
function bindFloating(state) {
  if (state.removers.length > 0) return;
  const { panel, triggerEl, dotnet } = state;
  on(document, "pointerdown", (e) => {
    const target = e.target;
    if (target instanceof Node && !panel.contains(target) && !triggerEl.contains(target)) {
      void dotnet.invokeMethodAsync("OnWaxPopoverOutsidePointer");
    }
  }, state.removers);
  on(document, "keydown", (e) => {
    if (e.key === "Escape") void dotnet.invokeMethodAsync("OnWaxPopoverEscape");
  }, state.removers);
  on(window, "scroll", () => {
    const applied = applyPosition(state);
    if (applied !== state.applied) {
      state.applied = applied;
      void state.dotnet.invokeMethodAsync("OnWaxPopoverAppliedPlacement", applied);
    }
  }, state.removers, true);
  on(window, "resize", () => applyPosition(state), state.removers);
}
function unbindFloating(state) {
  cancelCloseTimer(state);
  for (const remove of state.removers) remove();
  state.removers = [];
}
function fitsOnSide(side, rect, pw, ph, offset) {
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  const m = VIEWPORT_MARGIN;
  switch (side) {
    case "bottom":
      return rect.bottom + offset + ph <= vh - m;
    case "top":
      return rect.top - offset - ph >= m;
    case "right":
      return rect.right + offset + pw <= vw - m;
    case "left":
      return rect.left - offset - pw >= m;
  }
}
function applyPosition(state) {
  const { triggerEl, panel, arrow, opts } = state;
  const rect = triggerEl.getBoundingClientRect();
  const pw = panel.offsetWidth;
  const ph = panel.offsetHeight;
  const vw = window.innerWidth;
  const vh = window.innerHeight;
  const m = VIEWPORT_MARGIN;
  const offset = opts.offset;
  const [sideToken, alignToken] = opts.placement.split("-");
  let side = sideToken;
  const align = alignToken ?? "center";
  const opposite = { top: "bottom", bottom: "top", left: "right", right: "left" };
  if (!fitsOnSide(side, rect, pw, ph, offset) && fitsOnSide(opposite[side], rect, pw, ph, offset)) {
    side = opposite[side];
  }
  const mainCoord = () => {
    switch (side) {
      case "bottom":
        return rect.bottom + offset;
      case "top":
        return rect.top - offset - ph;
      case "right":
        return rect.right + offset;
      case "left":
        return rect.left - offset - pw;
    }
  };
  const crossCoord = () => {
    if (side === "top" || side === "bottom") {
      if (align === "start") return rect.left;
      if (align === "end") return rect.right - pw;
      return rect.left + rect.width / 2 - pw / 2;
    }
    if (align === "start") return rect.top;
    if (align === "end") return rect.bottom - ph;
    return rect.top + rect.height / 2 - ph / 2;
  };
  let x;
  let y;
  if (side === "top" || side === "bottom") {
    y = mainCoord();
    x = crossCoord();
    y = clamp(y, m, vh - m - ph);
    x = clamp(x, m, vw - m - pw);
  } else {
    x = mainCoord();
    y = crossCoord();
    x = clamp(x, m, vw - m - pw);
    y = clamp(y, m, vh - m - ph);
  }
  panel.style.left = `${Math.round(x)}px`;
  panel.style.top = `${Math.round(y)}px`;
  if (arrow !== null) {
    if (side === "top" || side === "bottom") {
      const a = clamp(rect.left + rect.width / 2 - x - ARROW / 2, 8, pw - 8 - ARROW);
      arrow.style.left = `${Math.round(a)}px`;
      arrow.style.right = "";
      if (side === "bottom") {
        arrow.style.top = `${-ARROW / 2}px`;
        arrow.style.bottom = "";
      } else {
        arrow.style.bottom = `${-ARROW / 2}px`;
        arrow.style.top = "";
      }
    } else {
      const a = clamp(rect.top + rect.height / 2 - y - ARROW / 2, 8, ph - 8 - ARROW);
      arrow.style.top = `${Math.round(a)}px`;
      arrow.style.bottom = "";
      if (side === "right") {
        arrow.style.left = `${-ARROW / 2}px`;
        arrow.style.right = "";
      } else {
        arrow.style.right = `${-ARROW / 2}px`;
        arrow.style.left = "";
      }
    }
  }
  return align === "center" ? side : `${side}-${align}`;
}
function sync(trigger, panel, arrow, dotnet, options) {
  let state = states.get(panel);
  if (!state) {
    state = {
      triggerEl: trigger,
      panel,
      arrow,
      dotnet,
      opts: options,
      boundTrigger: null,
      applied: null,
      removers: [],
      triggerRemovers: [],
      closeTimer: null
    };
    states.set(panel, state);
  }
  state.triggerEl = trigger;
  state.arrow = arrow;
  state.opts = options;
  if (state.boundTrigger !== options.trigger) {
    bindTrigger(state);
    state.boundTrigger = options.trigger;
  }
  if (options.open) {
    bindFloating(state);
    const applied = applyPosition(state);
    state.applied = applied;
    return applied;
  }
  unbindFloating(state);
  state.applied = null;
  return null;
}
function detach(panel) {
  const state = states.get(panel);
  if (!state) return;
  unbindFloating(state);
  for (const remove of state.triggerRemovers) remove();
  states.delete(panel);
}
export {
  detach,
  sync
};
