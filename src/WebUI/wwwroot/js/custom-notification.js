
function ShowNotification(message, type = 0) {
    
    var color = '#67b173';
    if (type === 1) {
        color = '#f17171';
    }

    Toastify({
        text: message,
        duration: 6000,
        newWindow: true,
        close: true,
        gravity: "bottom", // `top` or `bottom`
        position: "right", // `left`, `center` or `right`
        stopOnFocus: true, // Prevents dismissing of toast on hover

        style: {
            //background: "linear-gradient(to right, #00b09b, #96c93d)",
            background: color,
        },
        
    }).showToast();
}
