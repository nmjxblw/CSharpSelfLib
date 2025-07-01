from transformers import AutoModelForCausalLM, AutoTokenizer, pipeline
import logging

class LLMProgram():
    def __init__(self, model_name: str = "gpt2", device: str = "cpu"):

        self.model_name = model_name
        self.device = device
        self.tokenizer = AutoTokenizer.from_pretrained(model_name)
        self.model = AutoModelForCausalLM.from_pretrained(model_name).to(device)
        self.pipeline = pipeline("text-generation", model=self.model, tokenizer=self.tokenizer, device=0 if device == "cuda" else -1)
        logging.basicConfig(level=logging.INFO)
        self.logger = logging.getLogger(__name__)

    def generate_text(self, prompt: str, max_length: int = 50) -> str:
        self.logger.info(f"Generating text for prompt: {prompt}")
        inputs = self.tokenizer(prompt, return_tensors="pt").to(self.device)
        outputs = self.model.generate(inputs.input_ids, max_length=max_length, num_return_sequences=1)
        generated_text = self.tokenizer.decode(outputs[0], skip_special_tokens=True)
        self.logger.info(f"Generated text: {generated_text}")
        return generated_text


if __name__ == "__main__":
    pass