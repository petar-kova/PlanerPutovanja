document.addEventListener("DOMContentLoaded", function () {
    initializeDestinationFilters();
});

function initializeDestinationFilters() {
    const searchInput = document.getElementById("destinationSearch");
    const filterButtons = document.querySelectorAll(".filter-btn");
    const destinationItems = document.querySelectorAll(".destination-item");
    const counter = document.getElementById("destinationCounter");
    const noResultsMessage = document.getElementById("noDestinationsMessage");

    if (!searchInput || filterButtons.length === 0 || destinationItems.length === 0) {
        return;
    }

    let activeFilter = "sve";

    function filterDestinations() {
        const searchTerm = searchInput.value.toLowerCase().trim();
        let visibleCount = 0;

        destinationItems.forEach(function (item) {
            const destinationName = item.dataset.name.toLowerCase();
            const destinationCategory = item.dataset.category.toLowerCase();

            const matchesSearch = destinationName.includes(searchTerm);
            const matchesFilter = activeFilter === "sve" || destinationCategory.includes(activeFilter);

            if (matchesSearch && matchesFilter) {
                item.style.display = "block";
                visibleCount++;
            } else {
                item.style.display = "none";
            }
        });

        if (counter) {
            counter.textContent = "Prikazano je " + visibleCount + " destinacija.";
        }

        if (noResultsMessage) {
            if (visibleCount === 0) {
                noResultsMessage.classList.add("show");
            } else {
                noResultsMessage.classList.remove("show");
            }
        }
    }

    searchInput.addEventListener("input", filterDestinations);

    filterButtons.forEach(function (button) {
        button.addEventListener("click", function () {
            filterButtons.forEach(function (btn) {
                btn.classList.remove("active");
            });

            button.classList.add("active");
            activeFilter = button.dataset.filter;
            filterDestinations();
        });
    });

    filterDestinations();
}
document.addEventListener("DOMContentLoaded", function () {
    initializeTripPlanner();
});

function initializeTripPlanner() {
    const generateButton = document.getElementById("generatePlanBtn");
    const destinationSelect = document.getElementById("plannerDestination");
    const daysInput = document.getElementById("plannerDays");
    const styleSelect = document.getElementById("plannerStyle");
    const tempoSelect = document.getElementById("plannerTempo");
    const resultContainer = document.getElementById("plannerResult");
    const destinationTitle = document.getElementById("planDestinationTitle");

    if (!generateButton || !destinationSelect || !daysInput || !styleSelect || !tempoSelect || !resultContainer) {
        return;
    }

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
        const destination = destinationSelect.value;
        const days = parseInt(daysInput.value);
        const style = styleSelect.value;
        const tempo = tempoSelect.value;

        if (isNaN(days) || days < 1 || days > 14) {
            resultContainer.innerHTML = `
                <div class="plan-summary">
                    <strong>Neispravan broj dana</strong>
                    <p>Molimo unesi broj dana između 1 i 14.</p>
                </div>
            `;
            return;
        }

        if (destinationTitle) {
            destinationTitle.textContent = destination;
        }

        const activities = activityIdeas[style];
        let planHtml = `
            <div class="plan-summary">
                <strong>${destination} • ${days} dana • ${style} putovanje</strong>
                <p>
                    Generiran je prijedlog plana s ${tempo}m tempom. Plan možeš koristiti kao početnu ideju
                    i dodatno ga prilagoditi prema vlastitim željama.
                </p>
            </div>
        `;

        for (let day = 1; day <= days; day++) {
            planHtml += `
                <div class="day-plan">
                    <h3>Dan ${day}</h3>
                    <ul>
                        <li>Jutro: ${activities[0]}</li>
                        <li>Popodne: ${activities[1]}</li>
                        <li>Večer: ${activities[2]}</li>
                    </ul>
                </div>
            `;
        }

        resultContainer.innerHTML = planHtml;
    });
}
document.addEventListener("DOMContentLoaded", function () {
    initializeBudgetCalculator();
});

function initializeBudgetCalculator() {
    const calculateButton = document.getElementById("calculateBudgetBtn");

    if (!calculateButton) {
        return;
    }

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

    function formatCurrency(value) {
        return value.toFixed(2).replace(".", ",") + " €";
    }

    function calculateBudget() {
        const people = parseInt(peopleInput.value) || 1;
        const days = parseInt(daysInput.value) || 1;
        const accommodationPerNight = parseFloat(accommodationInput.value) || 0;
        const foodPerPersonDay = parseFloat(foodInput.value) || 0;
        const transport = parseFloat(transportInput.value) || 0;
        const extra = parseFloat(extraInput.value) || 0;

        const nights = Math.max(days - 1, 1);
        const accommodationTotal = accommodationPerNight * nights;
        const foodTotal = foodPerPersonDay * people * days;
        const total = accommodationTotal + foodTotal + transport + extra;
        const perPerson = total / people;

        totalElement.textContent = formatCurrency(total);
        perPersonElement.textContent = formatCurrency(perPerson);
        accommodationElement.textContent = formatCurrency(accommodationTotal);
        foodElement.textContent = formatCurrency(foodTotal);
        transportElement.textContent = formatCurrency(transport);
        extraElement.textContent = formatCurrency(extra);
    }

    calculateButton.addEventListener("click", calculateBudget);

    const inputs = [
        peopleInput,
        daysInput,
        accommodationInput,
        foodInput,
        transportInput,
        extraInput
    ];

    inputs.forEach(function (input) {
        input.addEventListener("input", calculateBudget);
    });

    calculateBudget();
}
document.addEventListener("DOMContentLoaded", function () {
    initializeContactForm();
});

function initializeContactForm() {
    const sendButton = document.getElementById("sendContactBtn");

    if (!sendButton) {
        return;
    }

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
        nameError.textContent = "";
        emailError.textContent = "";
        subjectError.textContent = "";
        messageError.textContent = "";
        successMessage.classList.remove("show");
    }

    function isValidEmail(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    }

    sendButton.addEventListener("click", function () {
        clearErrors();

        let isValid = true;

        if (nameInput.value.trim().length < 2) {
            nameError.textContent = "Ime mora imati najmanje 2 znaka.";
            isValid = false;
        }

        if (!isValidEmail(emailInput.value.trim())) {
            emailError.textContent = "Unesi ispravnu email adresu.";
            isValid = false;
        }

        if (subjectInput.value === "") {
            subjectError.textContent = "Odaberi temu upita.";
            isValid = false;
        }

        if (messageInput.value.trim().length < 10) {
            messageError.textContent = "Poruka mora imati najmanje 10 znakova.";
            isValid = false;
        }

        if (isValid) {
            successMessage.classList.add("show");

            nameInput.value = "";
            emailInput.value = "";
            subjectInput.value = "";
            messageInput.value = "";
        }
    });
}
// ==============================
// MOJA PUTOVANJA - 3D TILT
// Scoped animacija samo za trip kartice
// ==============================

document.addEventListener("DOMContentLoaded", function () {
    const tripCards = document.querySelectorAll(".trip-card");

    tripCards.forEach(card => {
        card.addEventListener("mousemove", function (event) {
            const rect = card.getBoundingClientRect();

            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;

            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            const rotateX = ((y - centerY) / centerY) * -3.5;
            const rotateY = ((x - centerX) / centerX) * 3.5;

            card.style.transform =
                `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-10px)`;
        });

        card.addEventListener("mouseleave", function () {
            card.style.transform = "";
        });
    });
});