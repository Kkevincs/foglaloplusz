document.addEventListener("DOMContentLoaded", () => loadWorkers());

async function loadWorkers() {
    try {
        const workers = await getWorkers();
        const container = document.getElementById("workerContainer");
        if (!container) return;
        container.innerHTML = '';
        if (workers.length === 0) {
            container.innerHTML = '<p style="text-align: center; color: var(--text-light);">No team members found.</p>';
            return;
        }
        workers.forEach(worker => {
            const id = worker.id ?? worker.Id;
            const name = worker.name ?? worker.Name;
            const email = worker.email ?? worker.Email;
            const card = document.createElement('div');
            card.className = 'service-detail';
            card.setAttribute('data-worker-id', id);
            const initial = (name || '')[0]?.toUpperCase() || '👤';
            const esc = t => (t || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
            card.innerHTML = `
                <div class="service-icon">${esc(initial)}</div>
                <h2>${esc(name)}</h2>
                <p style="color: var(--primary-color); font-weight: 500;">Team Member</p>
                <p style="font-size: 0.9rem; color: var(--text-light);">${esc(email)}</p>
            `;
            container.appendChild(card);
        });
    } catch (error) {
        console.error("Failed to load workers:", error);
        const container = document.getElementById("workerContainer");
        if (container) container.innerHTML = '<p style="text-align: center; color: var(--text-light);">Failed to load team members.</p>';
    }
}
