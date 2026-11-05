const states = /* @__PURE__ */ new WeakMap();
let modalCount = 0;
let lockedOverflow = null;
function lockScroll() {
  if (modalCount++ > 0) return;
  const root = document.documentElement;
  lockedOverflow = root.style.overflow;
  root.style.overflow = "hidden";
}
function unlockScroll() {
  if (modalCount === 0 || --modalCount > 0) return;
  document.documentElement.style.overflow = lockedOverflow ?? "";
  lockedOverflow = null;
}
function on(target, type, handler, removers) {
  const listener = (e) => handler(e);
  target.addEventListener(type, listener);
  removers.push(() => target.removeEventListener(type, listener));
}
function teardown(dialog) {
  const state = states.get(dialog);
  if (!state) return;
  states.delete(dialog);
  for (const remove of state.removers) remove();
}
function closeOut(dialog, notify) {
  const state = states.get(dialog);
  if (!state) return;
  teardown(dialog);
  if (state.modal) unlockScroll();
  if (notify) void state.dotnet.invokeMethodAsync("OnWaxDialogClosed");
}
function show(dialog, dotnet, options) {
  if (dialog.open || states.has(dialog)) return;
  const removers = [];
  states.set(dialog, { removers, modal: options.modal, dotnet });
  on(dialog, "cancel", (e) => {
    e.preventDefault();
    void dotnet.invokeMethodAsync("OnWaxDialogCancel");
  }, removers);
  on(dialog, "close", () => closeOut(dialog, true), removers);
  if (options.modal) {
    on(dialog, "click", (e) => {
      if (options.closeOnBackdrop && e.target === dialog) {
        void dotnet.invokeMethodAsync("OnWaxDialogBackdropClick");
      }
    }, removers);
    lockScroll();
    dialog.showModal();
  } else {
    on(dialog, "keydown", (e) => {
      if (e.key === "Escape") {
        void dotnet.invokeMethodAsync("OnWaxDialogCancel");
      }
    }, removers);
    dialog.show();
  }
}
function hide(dialog) {
  if (dialog.open) {
    dialog.close();
  } else {
    closeOut(dialog, true);
  }
}
function dispose(dialog) {
  if (!states.has(dialog)) return;
  closeOut(dialog, false);
  if (dialog.open) dialog.close();
}
export {
  dispose,
  hide,
  show
};
