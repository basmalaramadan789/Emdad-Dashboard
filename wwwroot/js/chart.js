//$(document).ready(function () {
//    Morris.Bar({
//        element: 'bar-charts',
//        data: [{
//            y: '2006',
//            a: 100,
//            b: 90
//        }, {
//            y: '2007',
//            a: 75,
//            b: 65
//        }, {
//            y: '2008',
//            a: 50,
//            b: 40
//        }, {
//            y: '2009',
//            a: 75,
//            b: 65
//        }, {
//            y: '2010',
//            a: 50,
//            b: 40
//        }, {
//            y: '2011',
//            a: 75,
//            b: 65
//        }, {
//            y: '2012',
//            a: 100,
//            b: 90
//        }],
//        xkey: 'y',
//        ykeys: ['a', 'b'],
//        labels: ['Total Income', 'Total Outcome'],
//        lineColors: ['#ff9b44', '#fc6075'],
//        lineWidth: '3px',
//        barColors: ['#ff9b44', '#fc6075'],
//        resize: true,
//        redraw: true
//    });
//    Morris.Line({
//        element: 'line-charts',
//        data: [{
//            y: '2006',
//            a: 50,
//            b: 90
//        }, {
//            y: '2007',
//            a: 75,
//            b: 65
//        }, {
//            y: '2008',
//            a: 50,
//            b: 40
//        }, {
//            y: '2009',
//            a: 75,
//            b: 65
//        }, {
//            y: '2010',
//            a: 50,
//            b: 40
//        }, {
//            y: '2011',
//            a: 75,
//            b: 65
//        }, {
//            y: '2012',
//            a: 100,
//            b: 50
//        }],
//        xkey: 'y',
//        ykeys: ['a', 'b'],
//        labels: ['Total Sales', 'Total Revenue'],
//        lineColors: ['#ff9b44', '#fc6075'],
//        lineWidth: '3px',
//        resize: true,
//        redraw: true
//    });
//});

$(document).ready(function () {
    // Fetch bar chart data from the backend
 
    $.ajax({
        url: '/GetAttendanceStatistics', // API endpoint for bar chart
        method: 'GET',
        success: function (data) {
            if (data.length > 0) {
                // Hide the loading message
                $('#bar-charts').html('');
                $('#pie-chart').html('');

                // Bar Chart using Chart.js
                const barCtx = document.getElementById('bar-charts').getContext('2d');
                new Chart(barCtx, {
                    type: 'bar',
                    data: {
                        labels: data.map(item => item.y), // X-axis (dates)
                        datasets: [{
                            label: 'Present',
                            data: data.map(item => item.a), // Y-axis value for "Present"
                            backgroundColor: '#4caf50', // Green for present
                        }, {
                            label: 'Absent',
                            data: data.map(item => item.b), // Y-axis value for "Absent"
                            backgroundColor: 'yellow', // Yellow for absent
                        }, {
                            label: 'Off',
                            data: data.map(item => item.c), // Y-axis value for "Off"
                            backgroundColor: 'red', // Red for off
                        }]
                    },
                    options: {
                        responsive: true,
                        scales: {
                            x: {
                                beginAtZero: true
                            },
                            y: {
                                beginAtZero: true
                            }
                        },
                        animation: {
                            duration: 1000, // Animation duration for smoother transitions
                        },
                        plugins: {
                            legend: {
                                position: 'bottom'
                            }
                        }
                    }
                });

                // Pie Chart using Chart.js
                
            } else {
                console.warn('No data available for the charts.');
            }
        },
        error: function (error) {
            console.error('Error loading chart data:', error);
        }
    });


    // Fetch line chart data from the backend
    $.ajax({
        url: '/api/charts/line-data', // API endpoint for line chart
        method: 'GET',
        success: function (data) {
            Morris.Line({
                element: 'line-charts',
                data: data,
                xkey: 'y',
                ykeys: ['a', 'b'],
                labels: ['Total Sales', 'Total Revenue'],
                lineColors: ['#ff9b44', '#fc6075'],
                lineWidth: '3px',
                resize: true,
                redraw: true
            });
        },
        error: function (error) {
            console.error('Error loading line chart data:', error);
        }
    });

    $.ajax({
        url: '/GetAttendanceStatistics', // API endpoint for pie chart data
        method: 'GET',
        success: function (data) {
            const ctx = document.getElementById('pie-chart').getContext('2d');

            // Sum up the Present, Absent, and Off counts from the data
            const presentCount = data.reduce((sum, item) => sum + item.a, 0);
            const absentCount = data.reduce((sum, item) => sum + item.b, 0);
            const offCount = data.reduce((sum, item) => sum + item.c, 0);

            // Create the pie chart
            new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: ['Present', 'Absent', 'Off'], // Categories
                    datasets: [{
                        data: [presentCount, absentCount, offCount], // Data for Present, Absent, Off
                        backgroundColor: ['#4caf50', 'yellow', 'red'], // Corresponding colors
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'bottom'
                        }
                    }
                }
            });
        },
        error: function (error) {
            console.error('Error loading pie chart data:', error);
        }
    });



});
