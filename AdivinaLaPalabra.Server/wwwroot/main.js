document.getElementById("createButton").addEventListener("click", function () {
    // Genera un PIN de 4 dígitos 0000 to 9999
    sessionStorage.setItem("roomCode", Math.floor(Math.random() * 9999).toString().padStart(4, '0')); 

    saveName();
});

document.getElementById("joinButton").addEventListener("click", function () {
    sessionStorage.setItem("roomCode", document.getElementById("roomPIN").value);
    
    saveName();
});


function saveName() {
    const username = document.getElementById("usernameInput").value;
    sessionStorage.setItem("username", username);

    window.location.href = "/Draw/game.html";
}


