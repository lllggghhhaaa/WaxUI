const targets = /* @__PURE__ */ new WeakMap();
let windowDrop = null;
function on(target, type, handler, removers) {
  const listener = (event) => handler(event);
  target.addEventListener(type, listener);
  removers.push(() => target.removeEventListener(type, listener));
}
function hasFiles(event) {
  const types = event.dataTransfer?.types;
  return types ? Array.from(types).includes("Files") : false;
}
function inputFor(container) {
  return container.querySelector('input[type="file"]');
}
function fillInput(input, files, maxFiles) {
  if (!files || files.length === 0) {
    return;
  }
  const transfer = new DataTransfer();
  const count = Math.min(files.length, Math.max(1, maxFiles));
  for (let index = 0; index < count; index++) {
    transfer.items.add(files[index]);
  }
  input.files = transfer.files;
  input.dispatchEvent(new Event("change", { bubbles: true }));
}
function attachDropTarget(target, dotnet, maxFiles = 1) {
  detachDropTarget(target);
  const removers = [];
  const state = { removers, dragging: false, dotnet };
  targets.set(target, state);
  const setDragging = (value) => {
    if (state.dragging === value) {
      return;
    }
    state.dragging = value;
    void dotnet.invokeMethodAsync("OnDragStateChanged", value);
  };
  on(target, "dragenter", (event) => {
    if (!hasFiles(event)) return;
    event.preventDefault();
    setDragging(true);
  }, removers);
  on(target, "dragover", (event) => {
    if (!hasFiles(event)) return;
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = "copy";
  }, removers);
  on(target, "dragleave", (event) => {
    const next = event.relatedTarget;
    if (next && target.contains(next)) return;
    setDragging(false);
  }, removers);
  on(target, "drop", (event) => {
    event.preventDefault();
    setDragging(false);
    const input = inputFor(target);
    if (input) fillInput(input, event.dataTransfer?.files ?? null, maxFiles);
  }, removers);
}
function detachDropTarget(target) {
  const state = targets.get(target);
  if (!state) return;
  targets.delete(target);
  for (const remove of state.removers) remove();
}
function attachWindowDrop(root, dotnet, maxFiles) {
  detachWindowDrop();
  const removers = [];
  const state = { removers, depth: 0, visible: false, dotnet };
  windowDrop = state;
  const setVisible = (value) => {
    if (state.visible === value) {
      return;
    }
    state.visible = value;
    void dotnet.invokeMethodAsync("OnWindowDragStateChanged", value);
  };
  const hide = () => {
    state.depth = 0;
    setVisible(false);
  };
  on(window, "dragenter", (event) => {
    if (!hasFiles(event)) return;
    event.preventDefault();
    state.depth += 1;
    setVisible(true);
  }, removers);
  on(window, "dragover", (event) => {
    if (!hasFiles(event)) return;
    event.preventDefault();
    if (event.dataTransfer) event.dataTransfer.dropEffect = "copy";
  }, removers);
  on(window, "dragleave", (event) => {
    if (!hasFiles(event)) return;
    state.depth = Math.max(0, state.depth - 1);
    if (state.depth === 0) setVisible(false);
  }, removers);
  on(window, "drop", (event) => {
    hide();
    if (event.defaultPrevented) return;
    event.preventDefault();
    const input = inputFor(root);
    if (input) fillInput(input, event.dataTransfer?.files ?? null, maxFiles);
  }, removers);
  on(window, "dragend", hide, removers);
}
function detachWindowDrop() {
  if (!windowDrop) return;
  for (const remove of windowDrop.removers) remove();
  windowDrop = null;
}
function openPicker(container) {
  const input = inputFor(container);
  if (!input) return;
  input.value = "";
  input.click();
}
function resetInput(container) {
  const input = inputFor(container);
  if (input) input.value = "";
}
export {
  attachDropTarget,
  attachWindowDrop,
  detachDropTarget,
  detachWindowDrop,
  openPicker,
  resetInput
};
