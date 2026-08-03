// BeeDayCardMenu behaviors that cannot be expressed reliably in CSS alone:
//   1. Viewport-aware placement. This module only *measures* — the panel is
//      rendered once (invisible via the `--measuring` class) so its real
//      width/height are known, never estimated, and it is only revealed
//      after BeeDayCardMenu.razor.cs has computed and applied the final
//      placement via CardMenuPlacementCalculator (a pure C# function, unit
//      tested independently of any DOM/browser).
//   2. Outside-click detection via a real document-level listener. A CSS
//      full-viewport `position: fixed` scrim was tried first and rejected:
//      several ancestors in the card/board layout (entrance animations on
//      .levelup-sortable__item and .dashboard-page) carry a resting
//      `transform` value, which creates a new containing block for any
//      fixed-positioned descendant and silently confines it to that
//      ancestor's box instead of the viewport. A document listener sidesteps
//      that entirely.
const outsideClickRegistrations = new WeakMap();

export function measureGeometry(trigger, panel) {
    const t = trigger.getBoundingClientRect();
    const p = panel.getBoundingClientRect();

    return {
        triggerTop: t.top,
        triggerBottom: t.bottom,
        triggerLeft: t.left,
        triggerRight: t.right,
        menuWidth: p.width,
        menuHeight: p.height,
        viewportWidth: window.innerWidth,
        viewportHeight: window.innerHeight
    };
}

export function registerOutsideClick(container, dotnetReference) {
    if (!container || outsideClickRegistrations.has(container)) {
        return;
    }

    const handlePointerDown = event => {
        if (!container.contains(event.target)) {
            dotnetReference.invokeMethodAsync('NotifyOutsideClickAsync');
        }
    };

    // Capture phase: fires before any inner handler can stop propagation.
    document.addEventListener('pointerdown', handlePointerDown, true);
    outsideClickRegistrations.set(container, handlePointerDown);
}

export function unregisterOutsideClick(container) {
    const handler = outsideClickRegistrations.get(container);
    if (handler) {
        document.removeEventListener('pointerdown', handler, true);
        outsideClickRegistrations.delete(container);
    }
}
