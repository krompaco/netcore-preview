import { Controller } from "stimulus"

export default class extends Controller {
  connect() {
    this.element.textContent = "It works from Stimulus! Test.";
  }
}