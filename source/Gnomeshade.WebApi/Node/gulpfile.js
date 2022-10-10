var gulp = require('gulp');

var nodeRoot = './node_modules/';
var targetPath = './../wwwroot/lib/';

gulp.task('build', function (done) {
	gulp.src(nodeRoot + "bootstrap/dist/js/*").pipe(gulp.dest(targetPath + "/bootstrap/dist/js"));
	gulp.src(nodeRoot + "bootstrap/dist/css/*").pipe(gulp.dest(targetPath + "/bootstrap/dist/css"));
	gulp.src(nodeRoot + "bootstrap/dist/fonts/*").pipe(gulp.dest(targetPath + "/bootstrap/dist/fonts"));

	gulp.src(nodeRoot + "jquery/dist/jquery.js").pipe(gulp.dest(targetPath + "/jquery/dist"));
	gulp.src(nodeRoot + "jquery/dist/jquery.min.js").pipe(gulp.dest(targetPath + "/jquery/dist"));
	gulp.src(nodeRoot + "jquery/dist/jquery.min.map").pipe(gulp.dest(targetPath + "/jquery/dist"));

	gulp.src(nodeRoot + "jquery-validation/dist/*.js").pipe(gulp.dest(targetPath + "/jquery-validation/dist"));

	gulp.src(nodeRoot + "jquery-validation-unobtrusive/dist/*.js").pipe(gulp.dest(targetPath + "/jquery-validation-unobtrusive"));

	done();
});
