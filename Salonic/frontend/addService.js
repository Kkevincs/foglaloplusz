document.addEventListener("DOMContentLoaded", () => {
    setupAddServiceForm();
});

function setupAddServiceForm() {
    const addServiceForm = document.getElementById('addServiceForm');
    if (!addServiceForm) return;

    addServiceForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        const messageEl = document.getElementById('addServiceMessage');
        if (messageEl) { messageEl.textContent = ''; messageEl.style.color = ''; }

        const serviceName = document.getElementById('serviceName').value.trim();
        const serviceDescription = document.getElementById('serviceDescription').value.trim();
        const priceInput = document.getElementById('servicePrice').value;
        const servicePrice = parseInt(priceInput, 10);

        if (!serviceName || !serviceDescription || isNaN(servicePrice) || servicePrice < 0) {
            if (messageEl) { messageEl.textContent = 'Please fill in all fields with valid values.'; messageEl.style.color = 'red'; }
            return;
        }

        const newService = {
            Name: serviceName,
            Description: serviceDescription,
            Price: servicePrice
        };

        try {
            await createService(newService);
            if (messageEl) { messageEl.textContent = 'Service added successfully!'; messageEl.style.color = 'green'; }
            addServiceForm.reset();
            setTimeout(() => { window.location.href = 'services.html'; }, 1500);
        } catch (error) {
            console.error("Failed to create service:", error);
            if (messageEl) { messageEl.textContent = 'Failed to add service: ' + (error.message || 'Unknown error'); messageEl.style.color = 'red'; }
        }
    });
}
