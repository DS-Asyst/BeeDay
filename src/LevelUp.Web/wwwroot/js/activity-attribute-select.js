// ActivityAttributeSelect's only DOM-dependent behavior: measuring the
// trigger and the (invisibly rendered, real-layout) menu so ActivityAttributeSelect.razor.cs
// can compute a viewport-aware placement (AttributeSelectPlacementCalculator,
// pure C#, unit tested independently of any DOM/browser) before revealing it.
export function measureGeometry(trigger, menu) {
    const t = trigger.getBoundingClientRect();
    const m = menu.getBoundingClientRect();

    return {
        triggerTop: t.top,
        triggerBottom: t.bottom,
        triggerLeft: t.left,
        triggerWidth: t.width,
        menuHeight: m.height,
        viewportHeight: window.innerHeight
    };
}
