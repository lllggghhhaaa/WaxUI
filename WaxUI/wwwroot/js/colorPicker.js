function beginPointerDrag(element, pointerId) {
  element.setPointerCapture(pointerId);
  const rect = element.getBoundingClientRect();
  return { left: rect.left, top: rect.top, width: rect.width, height: rect.height };
}
function isEyedropperSupported() {
  return "EyeDropper" in window;
}
async function pickColorWithEyedropper() {
  const constructor = window.EyeDropper;
  if (!constructor) {
    return { color: null, supported: false };
  }
  try {
    const result = await new constructor().open();
    return { color: result.sRGBHex, supported: true };
  } catch {
    return { color: null, supported: true };
  }
}
async function copyText(text) {
  try {
    await navigator.clipboard.writeText(text);
    return true;
  } catch {
    return false;
  }
}
function blurElement(element) {
  element.blur();
}
function preventArrowKeyScroll(element) {
  if (element.dataset.waxArrowGuard === "true") {
    return;
  }
  element.dataset.waxArrowGuard = "true";
  element.addEventListener("keydown", (event) => {
    if (event.key.startsWith("Arrow")) {
      event.preventDefault();
    }
  });
}
export {
  beginPointerDrag,
  blurElement,
  copyText,
  isEyedropperSupported,
  pickColorWithEyedropper,
  preventArrowKeyScroll
};
