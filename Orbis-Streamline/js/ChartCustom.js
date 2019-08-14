(function (global) {
    var chart;
 
    function ChartLoad(sender, args) {
        chart = sender.get_kendoWidget(); //store a reference to the Kendo Chart widget, we will use its methods
    }
 
    global.chartLoad = ChartLoad;
 
    function resizeChart() {
        if (chart)
            chart.resize(); //redraw the chart so it takes the new size of its container when it changes (e.g., browser window size change, parent container size change)
    }
 
 
    //this logic ensures that the chart resizing will happen only once, at most - every 200ms
    //to prevent calling the handler too often if old browsers fire the window.onresize event multiple times
    var TO = false;
    window.onresize = function () {
        if (TO !== false)
            clearTimeout(TO);
        TO = setTimeout(resizeChart, 200);
    }
 
})(window);