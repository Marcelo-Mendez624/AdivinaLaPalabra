let currentWord = "";

export function randomWord() {
    var words = ["apple", "banana", "cat", "dog", "elephant", "flower", "guitar", "house", "ice cream", "jungle"];
    var randomIndex = Math.floor(Math.random() * words.length);
    return words[randomIndex];
}