function getCheckedItems()
{
    return localStorage.getItem("checked") ? 
            JSON.parse(localStorage.getItem("checked")) : [];
}

function setCheckedItems(array)
{
    localStorage.setItem("checked", JSON.stringify(array));
}

function addCheckedItems(id)
{
    let currentChecked = getCheckedItems();
    currentChecked.push(id);
    setCheckedItems(currentChecked);
}

function removeCheckedItems(id)
{
    let currentChecked = getCheckedItems();
    let index = currentChecked.indexOf(id);

    if(index !== -1)
        currentChecked.splice(index, 1);

    setCheckedItems(currentChecked);
}

document.addEventListener("DOMContentLoaded", () => 
{
    document
        .querySelectorAll('[type="checkbox"]')
        .forEach(checkbox => 
            checkbox.addEventListener("change", function() 
            {
                let id = parseInt(this.id.split("_")[1]);
                if(this.checked)
                {
                    addCheckedItems(id);
                } else
                {
                    removeCheckedItems(id);
                }
            }));

    getCheckedItems().forEach(item => {
        document.getElementById(`checkbox_${item}`).checked = true;
    });
});