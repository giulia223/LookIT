// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/* wwwroot/js/javascript.js */
$(document).ready(function () {
    // Previzualizare Imagine
    const imageInput = document.getElementById('imageInput');
    if (imageInput) {
        imageInput.addEventListener('change', function () {
            const file = this.files[0];
            const newImgContainer = document.getElementById('newImageContainer');
            const newImgPreview = document.getElementById('newImagePreview');

            if (file && newImgPreview) {
                newImgPreview.src = URL.createObjectURL(file);
                newImgContainer.classList.remove('d-none');

                const currentImg = document.getElementById('currentImageContainer');
                if (currentImg) currentImg.style.opacity = '0.5';
            }
        });
    }

    // Previzualizare Video
    const videoInput = document.getElementById('videoInput');
    if (videoInput) {
        videoInput.addEventListener('change', function () {
            const file = this.files[0];
            const newVidContainer = document.getElementById('newVideoContainer');
            const newVidPreview = document.getElementById('newVideoPreview');

            if (file && newVidPreview) {
                newVidPreview.src = URL.createObjectURL(file);
                newVidContainer.classList.remove('d-none');

                const currentVid = document.getElementById('currentVideoContainer');
                if (currentVid) currentVid.style.opacity = '0.5';
            }
        });
    }
});