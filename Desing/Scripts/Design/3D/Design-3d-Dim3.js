//DIM

class RayysLinearDimension {

    constructor(domRoot, renderer, rendererMenu, camera) {
        this.domRoot = domRoot;
        this.renderer = renderer;
        this.camera = camera;

        this.cb = {
            onChange: []
        };
        this.config = {
            headLength: 0.5,
            headWidth: 0.35,
            units: "mm",
            unitsConverter: function (v) {
                return v;
            }
        };
    }

    create(p0, p1, extrude) {

        this.from = p0;
        this.to = p1;
        this.extrude = extrude;

        this.node = new THREE.Object3D();
        this.hidden = undefined;

        let el = document.createElement("div");
        el.id = this.node.id;
        el.classList.add("dim");
        el.style.left = "100px";
        el.style.top = "100px";
        el.innerHTML = "";
        this.domRoot.appendChild(el);
        this.domElement = el;

        this.updateDimCamera(this.camera);

        return this.node;
    }

    updateDimCamera(camera) {
        this.camera = camera;

        // re-create arrow
        if (!this.node) {
            return;
        }
        this.node.children.length = 0;

        let p0 = this.from;
        let p1 = this.to;
        let extrude = this.extrude;

        var pmin, pmax;
        if (extrude.x >= 0 && extrude.y >= 0 && extrude.z >= 0) {
            pmax = new THREE.Vector3(
                extrude.x + Math.max(p0.x, p1.x),
                extrude.y + Math.max(p0.y, p1.y),
                extrude.z + Math.max(p0.z, p1.z));

            pmin = new THREE.Vector3(
                extrude.x < 1e-16 ? extrude.x + Math.min(p0.x, p1.x) : pmax.x,
                extrude.y < 1e-16 ? extrude.y + Math.min(p0.y, p1.y) : pmax.y,
                extrude.z < 1e-16 ? extrude.z + Math.min(p0.z, p1.z) : pmax.z);
        } else if (extrude.x <= 0 && extrude.y <= 0 && extrude.z <= 0) {
            pmax = new THREE.Vector3(
                extrude.x + Math.min(p0.x, p1.x),
                extrude.y + Math.min(p0.y, p1.y),
                extrude.z + Math.min(p0.z, p1.z));

            pmin = new THREE.Vector3(
                extrude.x > -1e-16 ? extrude.x + Math.max(p0.x, p1.x) : pmax.x,
                extrude.y > -1e-16 ? extrude.y + Math.max(p0.y, p1.y) : pmax.y,
                extrude.z > -1e-16 ? extrude.z + Math.max(p0.z, p1.z) : pmax.z);
        }

        var origin = pmax.clone().add(pmin).multiplyScalar(0.5);
        var dir = pmax.clone().sub(pmin);
        dir.normalize();

        var length = pmax.distanceTo(pmin) / 2;
        var hex = 0x0;
        var arrowHelper0 = new THREE.ArrowHelper(dir, origin, length, hex, this.config.headLength, this.config.headWidth);
        this.node.add(arrowHelper0);

        dir.negate();
        var arrowHelper1 = new THREE.ArrowHelper(dir, origin, length, hex, this.config.headLength, this.config.headWidth);
        this.node.add(arrowHelper1);

        // reposition label
        if (this.domElement !== undefined) {
            let textPos = origin.project(this.camera);

            let clientX = this.renderer.domElement.offsetWidth * (textPos.x + 1) / 2 - this.config.headLength + this.renderer.domElement.offsetLeft;

            let clientY = -this.renderer.domElement.offsetHeight * (textPos.y - 1) / 2 - this.config.headLength + this.renderer.domElement.offsetTop;

            let dimWidth = this.domElement.offsetWidth;
            let dimHeight = this.domElement.offsetHeight;

            this.domElement.style.left = `${clientX - dimWidth / 2}px`;
            this.domElement.style.top = `${clientY - dimHeight / 2}px`;

            this.domElement.innerHTML = `${this.config.unitsConverter(pmin.distanceTo(pmax)).toFixed(2)}${this.config.units}`;
        }
    }

    detach() {
        if (this.node && this.node.parent) {
            this.node.parent.remove(this.node);
        }
        if (this.domElement !== undefined) {
            this.domRoot.removeChild(this.domElement);
            this.domElement = undefined;
        }
    }
    facingCamera.cb.facingDirChange.push(function(event) {
        debugger;
        let facingDir = facingCamera.dirs[event.current.best];
        if (dim0.node !== undefined) {
            dim0.detach();
        }
        if (dim1.node !== undefined) {
            dim1.detach();
        }
        var bbox = newRect.geometry.boundingBox;
        if (Math.abs(facingDir.x) === 1) {
            let from = new THREE.Vector3(bbox.min.x, bbox.min.y, bbox.min.z);
            let to = new THREE.Vector3(bbox.max.x, bbox.min.y, bbox.max.z);
            let newDimension = dim0.create(from, to, facingDir);
            newRect.add(newDimension);
        }
        if (Math.abs(facingDir.z) === 1) {
            let from = new THREE.Vector3(bbox.min.x, bbox.min.y, bbox.min.z);
            let to = new THREE.Vector3(bbox.max.x, bbox.min.y, bbox.max.z);
            let newDimension = dim0.create(from, to, facingDir);
            newRect.add(newDimension);
        }
        if (Math.abs(facingDir.y) === 1) {
            let newArray = event.current.facing.slice();
            let bestIdx = newArray.indexOf(event.current.best);
            newArray.splice(bestIdx, 1);

            let facingDir0 = facingCamera.dirs[newArray[0]];
            let facingDir1 = facingCamera.dirs[newArray[1]];

            let from = new THREE.Vector3(bbox.min.x, bbox.min.y, bbox.min.z);
            let to = new THREE.Vector3(bbox.max.x, bbox.min.y, bbox.max.z);

            console.log(from, to);
            let newDimension0 = dim0.create(from, to, facingDir0);
            let newDimension1 = dim1.create(from, to, facingDir1);
            newRect.add(newDimension0);
            newRect.add(newDimension1);
        }

    });
}

class RayysFacingCamera {
    constructor() {
        // camera looking direction will be saved here
        this.dirVector = new THREE.Vector3();

        // all world directions
        this.dirs = [
            new THREE.Vector3(+1, 0, 0),
            new THREE.Vector3(-1, 0, 0),
            new THREE.Vector3(0, +1, 0),
            new THREE.Vector3(0, -1, 0),
            new THREE.Vector3(0, 0, +1),
            new THREE.Vector3(0, 0, -1)
        ];

        // index of best facing direction will be saved here
        this.facingDirs = [];
        this.bestFacingDir = undefined;

        // TODO: add other facing directions too

        // event listeners are collected here
        this.cb = {
            facingDirChange: []
        };
    }

    checkDim(camera) {
        camera.getWorldDirection(this.dirVector);
        this.dirVector.negate();

        var maxk = 0;
        var maxdot = -1e19;

        var oldFacingDirs = this.facingDirs;
        var facingDirsChanged = false;
        this.facingDirs = [];

        for (var k = 0; k < this.dirs.length; k++) {
            var dot = this.dirs[k].dot(this.dirVector);
            var angle = Math.acos(dot);
            if (angle > -Math.PI / 2 && angle < Math.PI / 2) {
                this.facingDirs.push(k);
                if (oldFacingDirs.indexOf(k) === -1) {
                    facingDirsChanged = true;
                }
                if (Math.abs(dot) > maxdot) {
                    maxdot = dot;
                    maxk = k;
                }
            }
        }

        // and if facing direction changed, notify subscribers
        if (maxk !== this.bestFacingDir || facingDirsChanged) {
            var prevDir = this.bestFacingDir;
            this.bestFacingDir = maxk;

            for (var i = 0; i < this.cb.facingDirChange.length; i++) {
                this.cb.facingDirChange[i]({
                    before: { facing: oldFacingDirs, best: prevDir },
                    current: { facing: this.facingDirs, best: this.bestFacingDir }
                }, this);
            }
        }
    }
}