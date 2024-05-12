// import json data
const file = await fetch("./data.json");
const data = await file.json();

// add event listener
document.getElementById("btnSearch").addEventListener("click", (e) => {
    const query = document.getElementById("search").value.toLowerCase();
    const searchOption = document.querySelector('input[name="searchOption"]:checked').value;
    const exactMatch = document.getElementById("exactMatchCheckbox").checked;

    const results = data.flat().filter(post => {
        let target;
        if (searchOption === "content") target = post.content.toLowerCase();
        else if (searchOption === "author") target = post.author.toLowerCase();
        else if (searchOption === "title") target = post.title.toLowerCase();

        if (exactMatch) {
            if (searchOption === "author") {
                return target === query;
            } else {
                return target.split(/\s+/).includes(query);
            }
        } else {
            return target.includes(query);
        }
    });

    // clear previous results
    const resultsList = document.getElementById("results");
    resultsList.innerHTML = "";

    if (results.length < 1) {
        resultsList.textContent = "No results found. Please try something else.";
    } else {
        // Sort results by the "date" column
        results.sort((a, b) => new Date(a.date) - new Date(b.date));

        // print each article
        results.forEach(result => {
            const article = document.createElement("div");
            article.classList.add("article");

            const entry = document.createElement("div");
            entry.classList.add("entry");
			
            const span = document.createElement("span");
            span.textContent = result.title;
            entry.appendChild(span);

            const articletop = document.createElement("div");
            articletop.classList.add("articletop");

            const author = document.createElement("div");
            author.classList.add("author");
            author.textContent = result.author;

            const date = document.createElement("div");
            date.classList.add("date");
            date.textContent = new Date(result.date.replace("T", " ")).toLocaleString();

            const content = document.createElement("div");
            content.classList.add("content");
            content.innerHTML = result.content;

            articletop.appendChild(author);
            articletop.appendChild(date);
            article.appendChild(entry);
            article.appendChild(articletop);
            article.appendChild(content);
            resultsList.appendChild(article);
        });
    }
});
