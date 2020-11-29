$(document).ready(function () {
    // Create/Edit post image
    let fileInput = $('.custom-form-group .file_input');
    let fileSpan = $('.custom-form-group .file__name');
    let fileSpanValue = fileSpan.text();

    fileInput.change(function(x){
        let fileName = x.target.value.split(/(\\|\/)/g).pop();

        if (fileName) {
            fileSpan.text(fileName);
        } else {
            fileSpan.text(fileSpanValue);
        }
    });

    // User image editor
    $('.user__image-input').change(function(){
        let input = this;
        let url = $(this).val();
        let ext = url.substring(url.lastIndexOf('.') + 1).toLowerCase();
        let userImage = $('.user__image');

        if (input.files && input.files[0] && (ext === "gif" || ext === "png" || ext === "jpeg" || ext === "jpg"))
        {
            let reader = new FileReader();

            reader.onload = function (e) {
                userImage.attr('src', e.target.result);
            }
            reader.readAsDataURL(input.files[0]);
        }
        else
        {
            userImage.attr('src', '/assets/no_preview.png');
        }
    });
})


