$(document).ready(function () {
    //$('.summernote').summernote({
    //    height: 300, // inaltimea editorului
    //    placeholder: 'Continut articol…',
    //    tabsize: 2,
    //    minHeight: 200,
    //    focus: true,
    //});
    $('.summernote').summernote({
        height: 250,
        dialogsInBody: true, // Crucial pentru a nu tăia ferestrele pop-up
        disableDragAndDrop: true,
        placeholder: 'Modifică mesajul tău aici...',
        fontNames: ['Arial', 'Arial Black', 'Comic Sans MS', 'Courier New', 'Helvetica', 'Impact', 'Tahoma', 'Times New Roman', 'Verdana'],
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'strikethrough', 'clear']],
            ['fontname', ['fontname']],
            ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph', 'height']],
            ['table', ['table']],
            ['insert', ['link', 'hr']],
            ['view', ['fullscreen', 'codeview', 'help']]
        ]
    });
});