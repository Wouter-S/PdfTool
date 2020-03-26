import Vue from "vue";
import vue2Dropzone from 'vue2-dropzone'
import 'vue2-dropzone/dist/vue2Dropzone.min.css'
Vue.config.productionTip = false;
import Axios from "Axios";


window.component = new Vue({
    el: "#app",
    components: {
        vueDropzone: vue2Dropzone
    },
    methods: {
        Combine: function () {
            Axios.post("/combine").then((response) => {
                console.log("OK");
                this.GetFiles();

            });
        },
        SplitFiles: function () {
            Axios.post("/split").then((response) => {
                console.log("OK");
                this.GetFiles();

            });
        },
        Download: function (name) {
            console.log(name);
            document.location.href = "/download/" + name;
        },
        DeleteFiles: function (name) {
            Axios.delete("/"+name).then((response) => {
                console.log("OK"); 
                this.GetFiles();
            });
        },
        GetFiles: function () {
            Axios.get("/files").then((response) => {
                console.log(response.data);
                this.uploads = response.data.uploads;
                this.processed = response.data.processed;
            });
        },
        HandleFileUploaded: function (name, xhr, test) {
            xhr.onload = () => {
                this.GetFiles();

                this.$refs.myVueDropzone.removeFile(name);//.$el.getElementsByClassName('dz-preview')
                //while (elements.length > 0) {
                //    elements[0].parentNode.classList.remove("dz-started");
                //    elements[0].parentNode.removeChild(elements[0]);
                    
                //}
            }
        },
        HandleError: function (file, message, xhr) {
            console.log(message);

        }
    },
  
    data: function() {
        return {
            dropzoneOptions: {
                url: '/upload',
                //thumbnailWidth: 150,
                //maxFilesize: 0.5,
                headers: { "My-Awesome-Header": "header value" }
            },
            uploads: [],
            processed:[]
        }
    },
    mounted() {
        this.GetFiles();

    },
    watch: {

    }
});
