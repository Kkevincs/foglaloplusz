document.addEventListener("DOMContentLoaded", () => {
    loadServices();
});

async function loadServices() {
    try {
        const services = await getServices();
        const serviceContainer = document.getElementById("serviceContainer");
        if (!serviceContainer) return;
        serviceContainer.innerHTML = '';
        services.forEach(service => {
            const serviceId = service.id ?? service.Id;
            const serviceName = service.name ?? service.Name;
            const serviceDescription = service.description ?? service.Description;
            const servicePrice = service.price ?? service.Price;
            if (!serviceId) return;
            const serviceCard = document.createElement('div');
            serviceCard.className = 'service-detail';
            serviceCard.dataset.serviceId = serviceId;
            serviceCard.innerHTML = `
                <button class="delete-service-btn" onclick="deleteServiceCard(${parseInt(serviceId)})" aria-label="Delete service">×</button>
                <div class="service-icon">💅</div>
                <h2>${escapeHtml(serviceName)}</h2>
                <p>${escapeHtml(serviceDescription)}</p>
                <p>${servicePrice} Ft</p>
            `;
            serviceContainer.appendChild(serviceCard);
        });
    } catch (error) {
        console.error("Failed to load services:", error);
        const serviceContainer = document.getElementById("serviceContainer");
        if (serviceContainer) {
            serviceContainer.innerHTML = '<p style="text-align: center; color: var(--text-light);">Failed to load services.</p>';
        }
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

window.deleteServiceCard = async function(serviceId) {
    const id = parseInt(serviceId);
    if (isNaN(id)) {
        alert('Invalid service ID');
        return;
    }
    if (!confirm('Are you sure you want to delete this service?')) return;
    try {
        await deleteService(id);
        const serviceCard = document.querySelector(`.service-detail[data-service-id="${id}"]`);
        if (serviceCard) {
            serviceCard.style.opacity = '0';
            serviceCard.style.transform = 'scale(0.9)';
            setTimeout(() => serviceCard.remove(), 300);
        }
    } catch (error) {
        console.error("Failed to delete service:", error);
        alert('Failed to delete service: ' + (error.message || 'Unknown error'));
    }
};
