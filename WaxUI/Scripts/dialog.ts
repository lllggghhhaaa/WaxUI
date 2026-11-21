interface WaxDialogCallback {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<void>;
}

export interface WaxDialogOpenOptions {
    modal: boolean;
    closeOnBackdrop: boolean;
}

interface DialogState {
    removers: (() => void)[];
    modal: boolean;
    dotnet: WaxDialogCallback;
    closing: boolean;
    cancelExit: (() => void) | null;
}

const states = new WeakMap<HTMLDialogElement, DialogState>();

let modalCount = 0;
let lockedOverflow: string | null = null;

function lockScroll(): void {
    if (modalCount++ > 0) return;
    const root = document.documentElement;
    lockedOverflow = root.style.overflow;
    root.style.overflow = "hidden";
}

function unlockScroll(): void {
    if (modalCount === 0 || --modalCount > 0) return;
    document.documentElement.style.overflow = lockedOverflow ?? "";
    lockedOverflow = null;
}

function on<T extends Event>(target: EventTarget, type: string, handler: (e: T) => void, removers: (() => void)[]): void {
    const listener: EventListener = (e) => handler(e as T);
    target.addEventListener(type, listener);
    removers.push(() => target.removeEventListener(type, listener));
}

function teardown(dialog: HTMLDialogElement): void {
    const state = states.get(dialog);
    if (!state) return;
    states.delete(dialog);
    for (const remove of state.removers) remove();
}

function closeOut(dialog: HTMLDialogElement, notify: boolean): void {
    const state = states.get(dialog);
    if (!state) return;
    teardown(dialog);
    if (state.modal) unlockScroll();
    if (notify) void state.dotnet.invokeMethodAsync("OnWaxDialogClosed");
}

export function show(
    dialog: HTMLDialogElement,
    dotnet: WaxDialogCallback,
    options: WaxDialogOpenOptions,
): void {
    const existing = states.get(dialog);
    if (dialog.open || existing) {
        // Reopened while the exit animation is still playing: cancel it so the
        // dialog never visibly flickers closed before staying up.
        existing?.cancelExit?.();
        return;
    }

    const removers: (() => void)[] = [];
    states.set(dialog, { removers, modal: options.modal, dotnet, closing: false, cancelExit: null });

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
        on(dialog, "keydown", (e: KeyboardEvent) => {
            if (e.key === "Escape") {
                void dotnet.invokeMethodAsync("OnWaxDialogCancel");
            }
        }, removers);
        dialog.show();
    }
}

export function hide(dialog: HTMLDialogElement): void {
    const state = states.get(dialog);
    if (!state) return;

    if (!dialog.open) {
        closeOut(dialog, true);
        return;
    }

    if (state.closing) return;
    state.closing = true;
    dialog.classList.add("wax-dialog-closing");

    // Exit animations (element, ::backdrop, card) are defined in app.css on
    // .wax-dialog-closing; close only once they have finished so the dialog is
    // never torn down mid-fade. The timer is the fallback when getAnimations
    // is unavailable or reports nothing.
    let finished = false;
    const finish = (): void => {
        if (finished) return;
        finished = true;
        dialog.classList.remove("wax-dialog-closing");
        dialog.close();
    };

    const timer = window.setTimeout(finish, 400);
    state.cancelExit = () => {
        if (finished) return;
        finished = true;
        window.clearTimeout(timer);
        state.closing = false;
        dialog.classList.remove("wax-dialog-closing");
    };

    const animations =
        typeof dialog.getAnimations === "function"
            ? dialog.getAnimations({ subtree: true }).map((a) => a.finished)
            : [];

    if (animations.length > 0) {
        void Promise.allSettled(animations).then(() => {
            window.clearTimeout(timer);
            finish();
        });
    }
}

export function dispose(dialog: HTMLDialogElement): void {
    const state = states.get(dialog);
    if (!state) return;
    state.cancelExit?.();
    closeOut(dialog, false);
    if (dialog.open) dialog.close();
}
