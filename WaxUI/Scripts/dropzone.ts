/* WaxDropzone / WaxDropOverlay browser support module.
 *
 * Loaded lazily through WaxModulePaths.Dropzone as ./_content/WaxUI/js/dropzone.g.js.
 *
 * Blazor drag events never expose the dropped files, so the module owns the drag/drop
 * listeners and hands dropped files to Blazor's <InputFile> by assigning the element's
 * FileList through DataTransfer and dispatching a change event. That keeps one contract
 * for both paths: the component always receives a real IBrowserFile and can stream it.
 *
 * - attachDropTarget / detachDropTarget: per-element zone listeners (WaxDropzone and the
 *   region-scoped overlay); reports the drag state back to Blazor so the target can render
 *   its highlight, and fills its file input on drop.
 * - attachWindowDrop / detachWindowDrop: page-wide listeners behind the screen-scoped drop
 *   overlay. Drops already handled by a zone arrive with defaultPrevented set and are ignored.
 * - openPicker / resetInput: the browse button and the Remove button.
 */

interface WaxDotNetCallback {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<void>;
}

interface DropTargetState {
    removers: (() => void)[];
    dragging: boolean;
    dotnet: WaxDotNetCallback;
}

interface WindowDropState {
    removers: (() => void)[];
    depth: number;
    visible: boolean;
    dotnet: WaxDotNetCallback;
}

const targets = new WeakMap<HTMLElement, DropTargetState>();

let windowDrop: WindowDropState | null = null;

function on<E extends Event>(
    target: EventTarget,
    type: string,
    handler: (event: E) => void,
    removers: (() => void)[],
): void {
    const listener: EventListener = (event) => handler(event as E);
    target.addEventListener(type, listener);
    removers.push(() => target.removeEventListener(type, listener));
}

function hasFiles(event: DragEvent): boolean {
    const types = event.dataTransfer?.types;
    return types ? Array.from(types).includes("Files") : false;
}

function inputFor(container: Element): HTMLInputElement | null {
    return container.querySelector<HTMLInputElement>('input[type="file"]');
}

/* Assigning input.files is the only way to hand dropped files to Blazor's InputFile:
 * its change handler reads the element's FileList and streams slices from it later. */
function fillInput(input: HTMLInputElement, files: FileList | null, maxFiles: number): void {
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

export function attachDropTarget(
    target: HTMLElement,
    dotnet: WaxDotNetCallback,
    maxFiles = 1,
): void {
    detachDropTarget(target);

    const removers: (() => void)[] = [];
    const state: DropTargetState = { removers, dragging: false, dotnet };
    targets.set(target, state);

    const setDragging = (value: boolean): void => {
        if (state.dragging === value) {
            return;
        }

        state.dragging = value;
        void dotnet.invokeMethodAsync("OnDragStateChanged", value);
    };

    on<DragEvent>(target, "dragenter", (event) => {
        if (!hasFiles(event)) return;

        // The browser refuses the drop unless every drag event is cancelled.
        event.preventDefault();
        setDragging(true);
    }, removers);

    on<DragEvent>(target, "dragover", (event) => {
        if (!hasFiles(event)) return;

        event.preventDefault();
        if (event.dataTransfer) event.dataTransfer.dropEffect = "copy";
    }, removers);

    on<DragEvent>(target, "dragleave", (event) => {
        // Moving over a child fires dragleave on the parent; stay highlighted.
        const next = event.relatedTarget as Node | null;
        if (next && target.contains(next)) return;

        setDragging(false);
    }, removers);

    on<DragEvent>(target, "drop", (event) => {
        event.preventDefault();
        setDragging(false);

        const input = inputFor(target);
        if (input) fillInput(input, event.dataTransfer?.files ?? null, maxFiles);
    }, removers);
}

export function detachDropTarget(target: HTMLElement): void {
    const state = targets.get(target);
    if (!state) return;

    targets.delete(target);
    for (const remove of state.removers) remove();
}

export function attachWindowDrop(root: HTMLElement, dotnet: WaxDotNetCallback, maxFiles: number): void {
    detachWindowDrop();

    const removers: (() => void)[] = [];
    const state: WindowDropState = { removers, depth: 0, visible: false, dotnet };
    windowDrop = state;

    const setVisible = (value: boolean): void => {
        if (state.visible === value) {
            return;
        }

        state.visible = value;
        void dotnet.invokeMethodAsync("OnWindowDragStateChanged", value);
    };

    const hide = (): void => {
        state.depth = 0;
        setVisible(false);
    };

    on<DragEvent>(window, "dragenter", (event) => {
        if (!hasFiles(event)) return;

        event.preventDefault();
        state.depth += 1;
        setVisible(true);
    }, removers);

    on<DragEvent>(window, "dragover", (event) => {
        if (!hasFiles(event)) return;

        event.preventDefault();
        if (event.dataTransfer) event.dataTransfer.dropEffect = "copy";
    }, removers);

    on<DragEvent>(window, "dragleave", (event) => {
        if (!hasFiles(event)) return;

        state.depth = Math.max(0, state.depth - 1);
        if (state.depth === 0) setVisible(false);
    }, removers);

    on<DragEvent>(window, "drop", (event) => {
        hide();

        // A drop zone inside the page already consumed this drop.
        if (event.defaultPrevented) return;

        event.preventDefault();
        const input = inputFor(root);
        if (input) fillInput(input, event.dataTransfer?.files ?? null, maxFiles);
    }, removers);

    // Dragging out of the window fires no final dragleave.
    on<DragEvent>(window, "dragend", hide, removers);
}

export function detachWindowDrop(): void {
    if (!windowDrop) return;

    for (const remove of windowDrop.removers) remove();
    windowDrop = null;
}

export function openPicker(container: HTMLElement): void {
    const input = inputFor(container);
    if (!input) return;

    // Clearing first lets the same file be picked again and still fire change.
    input.value = "";
    input.click();
}

export function resetInput(container: HTMLElement): void {
    const input = inputFor(container);
    if (input) input.value = "";
}
