"use strict";

(function () {
    const container = document.querySelector("[data-auction-id]");
    if (!container) return;

    const auctionId = parseInt(container.dataset.auctionId, 10);

    const timerEl = document.getElementById("timer");
    const priceEl = document.getElementById("currentPrice");
    const leaderEl = document.getElementById("currentLeader");
    const historyEl = document.getElementById("bidHistory");
    const bidPanel = document.getElementById("bidPanel");
    const bidError = document.getElementById("bidError");
    const placeBidBtn = document.getElementById("placeBid");
    const nameInput = document.getElementById("bidderName");
    const amountInput = document.getElementById("bidAmount");

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/auctionHub")
        .withAutomaticReconnect()
        .build();

    function formatTime(totalSeconds) {
        const m = Math.floor(totalSeconds / 60);
        const s = totalSeconds % 60;
        return m + ":" + String(s).padStart(2, "0");
    }

    connection.on("TimerTick", function (id, remaining) {
        if (id !== auctionId) return;
        timerEl.textContent = formatTime(remaining);
    });

    connection.on("BidPlaced", function (id, newPrice, leader, bidderName) {
        if (id !== auctionId) return;
        priceEl.textContent = Number(newPrice).toFixed(2);
        leaderEl.textContent = leader;

        const li = document.createElement("li");
        li.className = "list-group-item d-flex justify-content-between";
        li.innerHTML = "<span></span><span></span>";
        li.children[0].textContent = bidderName;
        li.children[1].textContent = Number(newPrice).toFixed(2) + " \u20AC";
        historyEl.prepend(li);
        while (historyEl.children.length > 10) historyEl.removeChild(historyEl.lastChild);

        bidError.textContent = "";
    });

    connection.on("BidRejected", function (reason) {
        bidError.textContent = reason;
    });

    connection.on("AuctionFinished", function (id, leader, price) {
        if (id !== auctionId) return;
        timerEl.textContent = "Ended";
        if (bidPanel) bidPanel.classList.add("d-none");
        if (leader) leaderEl.textContent = leader;
    });

    // Join the room on connect AND after every reconnect, so a dropped
    // connection re-subscribes to this auction's group automatically.
    async function joinRoom() {
        try {
            await connection.invoke("JoinAuction", auctionId);
        } catch (err) {
            console.error("JoinAuction failed:", err);
        }
    }
    connection.onreconnected(joinRoom);

    async function start() {
        try {
            await connection.start();
            await joinRoom();
        } catch (err) {
            console.error("Connection failed, retrying in 2s:", err);
            setTimeout(start, 2000);
        }
    }

    if (placeBidBtn) {
        placeBidBtn.addEventListener("click", async function () {
            bidError.textContent = "";
            const name = nameInput.value.trim();
            const amount = parseFloat(amountInput.value);

            if (!name) { bidError.textContent = "Enter your name."; return; }
            if (!(amount > 0)) { bidError.textContent = "Enter a valid bid amount."; return; }

            placeBidBtn.disabled = true;
            try {
                await connection.invoke("PlaceBid", auctionId, name, amount);
                amountInput.value = "";
            } catch (err) {
                bidError.textContent = "Could not place bid. Try again.";
                console.error(err);
            } finally {
                placeBidBtn.disabled = false;
            }
        });
    }

    start();
})();
