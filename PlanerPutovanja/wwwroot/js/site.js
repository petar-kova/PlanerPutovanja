// =========================================================
// PLANER PUTOVANJA - CLEAN PREMIUM SITE.JS
// Zamijeni CIJELI wwwroot/js/site.js ovim kodom.
// =========================================================

document.addEventListener("DOMContentLoaded", function () {
    initializeDestinationFilters();
    initializeTripPlanner();
    initializeBudgetCalculator();
    initializeContactForm();
    initializeTiltCards();
    initializeRevealAnimations();
    initializeMobileNavbarClose();
});

function initializeDestinationFilters() {
    const searchInput = document.getElementById("destinationSearch");
    const filterButtons = document.querySelectorAll(".filter-btn");
    const destinationItems = document.querySelectorAll(".destination-item");
    const counter = document.getElementById("destinationCounter");
    const noResultsMessage = document.getElementById("noDestinationsMessage") || document.querySelector(".destination-no-results") || document.querySelector(".no-results");

    if (!searchInput || filterButtons.length === 0 || destinationItems.length === 0) return;

    let activeFilter = "sve";
    const normalize = (value) => (value || "").toString().toLowerCase().trim();

    function filterDestinations() {
        const searchTerm = normalize(searchInput.value);
        let visibleCount = 0;

        destinationItems.forEach(function (item) {
            const destinationName = normalize(item.dataset.name || item.textContent);
            const destinationCategory = normalize(item.dataset.category);
            const matchesSearch = destinationName.includes(searchTerm);
            const matchesFilter = activeFilter === "sve" || destinationCategory.includes(activeFilter);

            item.style.display = matchesSearch && matchesFilter ? "" : "none";
            if (matchesSearch && matchesFilter) visibleCount++;
        });

        if (counter) counter.textContent = "Prikazano je " + visibleCount + " destinacija.";
        if (noResultsMessage) noResultsMessage.classList.toggle("show", visibleCount === 0);
    }

    searchInput.addEventListener("input", filterDestinations);

    filterButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            filterButtons.forEach((btn) => btn.classList.remove("active"));
            button.classList.add("active");
            activeFilter = normalize(button.dataset.filter) || "sve";
            filterDestinations();
        });
    });

    filterDestinations();
}

function initializeTripPlanner() {
    const generateButton = document.getElementById("generatePlanBtn");
    const destinationSelect = document.getElementById("plannerDestination");
    const daysInput = document.getElementById("plannerDays");
    const styleSelect = document.getElementById("plannerStyle");
    const tempoSelect = document.getElementById("plannerTempo");
    const resultContainer = document.getElementById("plannerResult");
    const destinationTitle = document.getElementById("planDestinationTitle");

    if (!generateButton || !destinationSelect || !daysInput || !styleSelect || !tempoSelect || !resultContainer) return;

    const activityIdeas = {
        "opušteno": [
            "lagana šetnja centrom i upoznavanje mjesta",
            "pauza za kavu ili ručak na lokalnoj lokaciji",
            "slobodno vrijeme za odmor i fotografiranje"
        ],
        "aktivno": [
            "obilazak glavnih znamenitosti",
            "duža šetnja ili rekreativna aktivnost",
            "večernji obilazak popularnih lokacija"
        ],
        "romantično": [
            "šetnja uz zalazak sunca",
            "večera u ugodnom restoranu",
            "posjet mirnijim i slikovitim lokacijama"
        ],
        "kulturno": [
            "obilazak muzeja ili povijesne jezgre",
            "posjet lokalnim znamenitostima",
            "upoznavanje tradicije i lokalne hrane"
        ],
        "avanturistički": [
            "aktivnost u prirodi ili adrenalinski sadržaj",
            "istraživanje manje poznatih lokacija",
            "fotografiranje vidikovaca i zanimljivih ruta"
        ]
    };

    generateButton.addEventListener("click", function () {
        const destination = destinationSelect.value || "Odabrana destinacija";
        const days = parseInt(daysInput.value, 10);
        const style = styleSelect.value || "opušteno";
        const tempo = tempoSelect.value || "normalni";

        if (isNaN(days) || days < 1 || days > 14) {
            resultContainer.innerHTML = `
                <div class="plan-summary">
                    <strong>Neispravan broj dana</strong>
                    <p>Unesi broj dana između 1 i 14.</p>
                </div>`;
            return;
        }

        if (destinationTitle) {
            destinationTitle.textContent = destination;
        }

        const activities = activityIdeas[style] || activityIdeas["opušteno"];

        let planHtml = `
            <div class="plan-summary">
                <strong>${escapeHtml(destination)} • ${days} dana • ${escapeHtml(style)} putovanje</strong>
                <p>Generiran je prijedlog plana s ${escapeHtml(tempo)} tempom.</p>
            </div>`;

        for (let day = 1; day <= days; day++) {
            planHtml += `
                <div class="day-plan">
                    <h3>Dan ${day}</h3>
                    <ul>
                        <li>Jutro: ${escapeHtml(activities[0])}</li>
                        <li>Popodne: ${escapeHtml(activities[1])}</li>
                        <li>Večer: ${escapeHtml(activities[2])}</li>
                    </ul>
                </div>`;
        }

        resultContainer.innerHTML = planHtml;
    });
}

function initializeBudgetCalculator() {
    const calculateButton = document.getElementById("calculateBudgetBtn");
    if (!calculateButton) return;

    const peopleInput = document.getElementById("peopleCount");
    const daysInput = document.getElementById("daysCount");
    const accommodationInput = document.getElementById("accommodationCost");
    const foodInput = document.getElementById("foodCost");
    const transportInput = document.getElementById("transportCost");
    const extraInput = document.getElementById("extraCost");

    const totalElement = document.getElementById("budgetTotal");
    const perPersonElement = document.getElementById("budgetPerPerson");
    const accommodationElement = document.getElementById("accommodationResult");
    const foodElement = document.getElementById("foodResult");
    const transportElement = document.getElementById("transportResult");
    const extraElement = document.getElementById("extraResult");

    function numberValue(input, fallback) {
        if (!input) return fallback;

        const value = parseFloat(String(input.value).replace(",", "."));
        return isNaN(value) ? fallback : value;
    }

    function formatCurrency(value) {
        return value.toFixed(2).replace(".", ",") + " €";
    }

    function calculateBudget() {
        const people = Math.max(parseInt(numberValue(peopleInput, 1), 10), 1);
        const days = Math.max(parseInt(numberValue(daysInput, 1), 10), 1);

        const accommodationTotal = numberValue(accommodationInput, 0) * Math.max(days - 1, 1);
        const foodTotal = numberValue(foodInput, 0) * people * days;
        const transport = numberValue(transportInput, 0);
        const extra = numberValue(extraInput, 0);

        const total = accommodationTotal + foodTotal + transport + extra;

        setText(totalElement, formatCurrency(total));
        setText(perPersonElement, formatCurrency(total / people));
        setText(accommodationElement, formatCurrency(accommodationTotal));
        setText(foodElement, formatCurrency(foodTotal));
        setText(transportElement, formatCurrency(transport));
        setText(extraElement, formatCurrency(extra));
    }

    [
        peopleInput,
        daysInput,
        accommodationInput,
        foodInput,
        transportInput,
        extraInput
    ]
        .filter(Boolean)
        .forEach((input) => input.addEventListener("input", calculateBudget));

    calculateButton.addEventListener("click", calculateBudget);

    calculateBudget();
}

function initializeContactForm() {
    const sendButton = document.getElementById("sendContactBtn");
    if (!sendButton) return;

    const nameInput = document.getElementById("contactName");
    const emailInput = document.getElementById("contactEmail");
    const subjectInput = document.getElementById("contactSubject");
    const messageInput = document.getElementById("contactMessage");

    const successMessage = document.getElementById("contactSuccessMessage");
    const nameError = document.getElementById("nameError");
    const emailError = document.getElementById("emailError");
    const subjectError = document.getElementById("subjectError");
    const messageError = document.getElementById("messageError");

    function clearErrors() {
        setText(nameError, "");
        setText(emailError, "");
        setText(subjectError, "");
        setText(messageError, "");

        if (successMessage) {
            successMessage.classList.remove("show");
        }
    }

    sendButton.addEventListener("click", function (event) {
        event.preventDefault();
        clearErrors();

        let isValid = true;

        if (!nameInput || nameInput.value.trim().length < 2) {
            setText(nameError, "Ime mora imati najmanje 2 znaka.");
            isValid = false;
        }

        if (!emailInput || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailInput.value.trim())) {
            setText(emailError, "Unesi ispravnu email adresu.");
            isValid = false;
        }

        if (!subjectInput || subjectInput.value === "") {
            setText(subjectError, "Odaberi temu upita.");
            isValid = false;
        }

        if (!messageInput || messageInput.value.trim().length < 10) {
            setText(messageError, "Poruka mora imati najmanje 10 znakova.");
            isValid = false;
        }

        if (isValid) {
            if (successMessage) {
                successMessage.classList.add("show");
            }

            [nameInput, emailInput, subjectInput, messageInput].forEach((input) => {
                if (input) input.value = "";
            });
        }
    });
}

function initializeTiltCards() {
    const cards = document.querySelectorAll(
        ".trip-card, .feature-3d-card, .destination-3d-card, .gallery-tile, .dashboard-mockup, .memories-hero-card, .premium-contact-card"
    );

    if (!cards.length) return;

    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const coarsePointer = window.matchMedia("(pointer: coarse)").matches;

    if (reducedMotion || coarsePointer) return;

    cards.forEach(function (card) {
        card.addEventListener("mousemove", function (event) {
            const rect = card.getBoundingClientRect();

            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;

            const rotateX = ((y - rect.height / 2) / (rect.height / 2)) * -3.5;
            const rotateY = ((x - rect.width / 2) / (rect.width / 2)) * 3.5;

            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-8px)`;
        });

        card.addEventListener("mouseleave", function () {
            card.style.transform = "";
        });
    });
}

function initializeRevealAnimations() {
    const candidates = document.querySelectorAll(
        ".section-title, .section-subtitle, .feature-3d-card, .step-3d-card, .trip-card, .dashboard-card, .details-panel, .create-trip-card, .side-card, .polaroid-card, .gallery-tile, .destination-3d-card, .memories-hero-card, .premium-contact-card"
    );

    if (!candidates.length) return;

    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    if (reducedMotion || !("IntersectionObserver" in window)) return;

    candidates.forEach((el) => el.classList.add("reveal-on-scroll"));

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add("is-visible");
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.12
    });

    candidates.forEach((el) => observer.observe(el));

    injectRevealStyles();
}

function injectRevealStyles() {
    if (document.getElementById("premiumRevealStyles")) return;

    const style = document.createElement("style");
    style.id = "premiumRevealStyles";
    style.textContent = `
        .reveal-on-scroll {
            opacity: 0;
            transform: translateY(18px);
            transition: opacity .55s ease, transform .55s ease;
        }

        .reveal-on-scroll.is-visible {
            opacity: 1;
            transform: translateY(0);
        }
    `;

    document.head.appendChild(style);
}

function initializeMobileNavbarClose() {
    const navbarCollapse = document.querySelector(".navbar-collapse");

    if (!navbarCollapse || typeof bootstrap === "undefined") return;

    navbarCollapse.querySelectorAll("a.nav-link").forEach(function (link) {
        link.addEventListener("click", function () {
            if (!navbarCollapse.classList.contains("show")) return;

            const collapseInstance =
                bootstrap.Collapse.getInstance(navbarCollapse) ||
                new bootstrap.Collapse(navbarCollapse, { toggle: false });

            collapseInstance.hide();
        });
    });
}

function setText(element, value) {
    if (element) {
        element.textContent = value;
    }
}

function escapeHtml(value) {
    return String(value)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}