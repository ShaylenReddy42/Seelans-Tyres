﻿$(function () {
    $('.viewReceiptForm').on('submit', function (event) {
        event.preventDefault();

        const orderId = $('#orderId', this).val();

        $.ajax({
            type: "post",
            url: "/Shopping/ViewReceipt",
            data: { orderId: orderId },
            success: function (receipt) {
                $('#viewReceiptModal').modal('show');
                $('#receipt').html(receipt);
            }
        });
    });
})