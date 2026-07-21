const registrations = new WeakMap();

export function initialize(container, dotnetReference) {
    if (!container || registrations.has(container)) {
        return;
    }

    let armedItem = null;
    let draggedItem = null;
    let dragClone = null;
    let dropTarget = null;
    let activePointerId = null;
    let startX = 0;
    let startY = 0;
    let lastX = 0;
    let lastY = 0;
    let longPressTimer = null;

    const itemFrom = element => element?.closest?.('[data-sortable-item]');
    const isInteractive = element => Boolean(element?.closest?.(
        'button, a, input, textarea, select, option, label, [contenteditable="true"], [role="button"], [data-no-drag]'));

    const clearLongPress = () => {
        if (longPressTimer !== null) {
            window.clearTimeout(longPressTimer);
            longPressTimer = null;
        }
    };

    const clearDropMarkers = () => {
        container.querySelectorAll('.is-drop-before, .is-drop-after')
            .forEach(element => element.classList.remove('is-drop-before', 'is-drop-after'));
    };

    const cleanup = () => {
        clearLongPress();
        clearDropMarkers();

        container.querySelectorAll('.is-dragging, .is-drag-armed')
            .forEach(element => element.classList.remove('is-dragging', 'is-drag-armed'));

        dragClone?.remove();
        dragClone = null;
        dropTarget = null;
        draggedItem = null;
        armedItem = null;
        activePointerId = null;
        document.body.classList.remove('levelup-is-sorting');
    };

    const updateClonePosition = (clientX, clientY) => {
        if (!dragClone) {
            return;
        }

        dragClone.style.transform = `translate3d(${clientX + 12}px, ${clientY + 12}px, 0)`;
    };

    const indicateTarget = (target, clientY) => {
        clearDropMarkers();

        if (!target || target === draggedItem) {
            return;
        }

        const rect = target.getBoundingClientRect();
        target.classList.add(clientY >= rect.top + rect.height / 2
            ? 'is-drop-after'
            : 'is-drop-before');
    };

    const resolveTarget = (clientX, clientY) => {
        const element = document.elementFromPoint(clientX, clientY);
        const candidate = itemFrom(element);
        dropTarget = candidate && container.contains(candidate) ? candidate : null;
        indicateTarget(dropTarget, clientY);
    };

    const beginDrag = () => {
        if (!armedItem || draggedItem) {
            return;
        }

        clearLongPress();
        draggedItem = armedItem;
        draggedItem.classList.remove('is-drag-armed');
        draggedItem.classList.add('is-dragging');
        document.body.classList.add('levelup-is-sorting');

        dragClone = draggedItem.cloneNode(true);
        dragClone.classList.remove(
            'is-dragging',
            'is-drag-armed',
            'is-drop-before',
            'is-drop-after',
            'levelup-card-enter',
            'levelup-card-leave',
            'is-reorder-settling');
        dragClone.classList.add('levelup-sortable__drag-clone');
        dragClone.removeAttribute('data-sortable-item');
        dragClone.removeAttribute('tabindex');
        dragClone.setAttribute('aria-hidden', 'true');
        const draggedRect = draggedItem.getBoundingClientRect();
        dragClone.style.width = `${draggedRect.width}px`;
        dragClone.style.height = `${draggedRect.height}px`;
        dragClone.style.margin = '0';
        dragClone.style.animation = 'none';
        dragClone.style.transition = 'none';
        document.body.appendChild(dragClone);

        updateClonePosition(lastX, lastY);
        resolveTarget(lastX, lastY);
    };

    const commit = async () => {
        const source = draggedItem;
        const target = dropTarget;
        const clientY = lastY;

        if (!source || !target || source === target) {
            cleanup();
            return;
        }

        const rect = target.getBoundingClientRect();
        const placeAfter = clientY >= rect.top + rect.height / 2;
        const itemId = source.dataset.sortableItem;
        const targetItemId = target.dataset.sortableItem;
        cleanup();

        if (itemId && targetItemId) {
            await dotnetReference.invokeMethodAsync('NotifyReorderAsync', itemId, targetItemId, placeAfter);
            window.requestAnimationFrame(() => {
                const moved = container.querySelector(`[data-sortable-item="${CSS.escape(itemId)}"]`);
                moved?.classList.add('is-reorder-settling');
                window.setTimeout(() => moved?.classList.remove('is-reorder-settling'), 240);
            });
        }
    };

    const onPointerDown = event => {
        if (event.button !== undefined && event.button !== 0) {
            return;
        }

        const item = itemFrom(event.target);
        if (!item || !container.contains(item) || isInteractive(event.target)) {
            return;
        }

        armedItem = item;
        activePointerId = event.pointerId;
        startX = lastX = event.clientX;
        startY = lastY = event.clientY;
        item.classList.add('is-drag-armed');
        item.setPointerCapture?.(event.pointerId);

        if (event.pointerType === 'touch' || event.pointerType === 'pen') {
            longPressTimer = window.setTimeout(beginDrag, 260);
        }
    };

    const onPointerMove = event => {
        if (activePointerId === null || event.pointerId !== activePointerId) {
            return;
        }

        lastX = event.clientX;
        lastY = event.clientY;
        const distance = Math.hypot(lastX - startX, lastY - startY);

        if (!draggedItem && event.pointerType === 'mouse' && distance >= 5) {
            beginDrag();
        }

        if (!draggedItem && (event.pointerType === 'touch' || event.pointerType === 'pen') && distance > 10) {
            clearLongPress();
            armedItem?.classList.remove('is-drag-armed');
            armedItem = null;
            return;
        }

        if (!draggedItem) {
            return;
        }

        event.preventDefault();
        updateClonePosition(lastX, lastY);
        resolveTarget(lastX, lastY);
    };

    const onPointerUp = event => {
        if (activePointerId === null || event.pointerId !== activePointerId) {
            return;
        }

        if (draggedItem) {
            event.preventDefault();
            void commit();
        } else {
            cleanup();
        }
    };

    const onPointerCancel = () => cleanup();

    const onKeyDown = event => {
        if ((event.key !== 'ArrowUp' && event.key !== 'ArrowDown') || isInteractive(event.target)) {
            return;
        }

        const item = itemFrom(event.target);
        const sibling = event.key === 'ArrowUp'
            ? item?.previousElementSibling
            : item?.nextElementSibling;

        if (!item || !sibling?.matches?.('[data-sortable-item]')) {
            return;
        }

        event.preventDefault();
        void dotnetReference.invokeMethodAsync(
            'NotifyReorderAsync',
            item.dataset.sortableItem,
            sibling.dataset.sortableItem,
            event.key === 'ArrowDown');
    };

    container.addEventListener('pointerdown', onPointerDown);
    container.addEventListener('pointermove', onPointerMove, { passive: false });
    container.addEventListener('pointerup', onPointerUp);
    container.addEventListener('pointercancel', onPointerCancel);
    container.addEventListener('keydown', onKeyDown);

    registrations.set(container, () => {
        container.removeEventListener('pointerdown', onPointerDown);
        container.removeEventListener('pointermove', onPointerMove);
        container.removeEventListener('pointerup', onPointerUp);
        container.removeEventListener('pointercancel', onPointerCancel);
        container.removeEventListener('keydown', onKeyDown);
        cleanup();
    });
}

export function dispose(container) {
    registrations.get(container)?.();
    registrations.delete(container);
}
