// emailListVue.js
new Vue({
    el: '#emailMessagesTable', // This should match the ID of your HTML element where you want to mount Vue.js
    data: {
        emailMessages: [], // Initialize with an empty array or fetch data from your server
        selectedStatus: '',
        emailStatusOptions: ['New', 'Work in Progress', 'Need More Detail', 'Done', 'Deleted']
    },
    methods: {
        messageStatusClass(status) {
            // Define CSS classes based on the status
            switch (status) {
                case 'New':
                    return 'status-new';
                case 'Work in Progress':
                    return 'status-work-in-progress';
                case 'Need More Detail':
                    return 'status-need-more-detail';
                case 'Done':
                    return 'status-done';
                case 'Deleted':
                    return 'status-deleted';
                default:
                    return '';
            }
        },
        messageStatusText(status) {
            // Define status text based on the status
            switch (status) {
                case 'New':
                    return 'New';
                case 'Work in Progress':
                    return 'Work in Progress';
                case 'Need More Detail':
                    return 'Need More Detail';
                case 'Done':
                    return 'Done';
                case 'Deleted':
                    return 'Deleted';
                default:
                    return '';
            }
        },
        formatSentTime(sentTime) {
            // Format sentTime as needed
            return sentTime;
        },
        downloadAttachment(id) {
            // Implement download attachment logic
            // You can use JavaScript to trigger the download here
        },
        changeStatus(id, newStatus) {
            // Implement change status logic
            // You can use JavaScript to make a POST request to change the status here
        },
        deleteMessage(id) {
            // Implement delete message logic
            // You can use JavaScript to show a confirmation modal and delete the message if confirmed
        }
    }
});
